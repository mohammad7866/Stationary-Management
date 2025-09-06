// frontend/src/components/LowStockAlertModal.jsx
import React, { useEffect, useState, useRef } from "react";
import { http } from "../lib/api";

// ---- styles ----
const backdrop = {
  position: "fixed",
  inset: 0,
  background: "rgba(0,0,0,0.35)",
  display: "flex",
  alignItems: "center",
  justifyContent: "center",
  zIndex: 9999,
};

const modal = {
  background: "white",
  borderRadius: 12,
  padding: 20,
  maxWidth: 800,
  width: "90%",
  boxShadow: "0 10px 30px rgba(0,0,0,0.25)",
};

const th = { textAlign: "left", padding: 8, borderBottom: "1px solid #eee" };
const td = { padding: 8, borderBottom: "1px solid #f5f5f5" };

/**
 * Shows low-stock popup every time the user logs in.
 * Triggers when `token` becomes truthy, and only for Admin/SuperAdmin.
 */
export default function LowStockAlertModal({ roles = [], token }) {
  const [visible, setVisible] = useState(false);
  const [rows, setRows] = useState([]);
  const lastToken = useRef(null);

  const isAllowed =
    Array.isArray(roles) &&
    (roles.includes("Admin") || roles.includes("SuperAdmin"));

  useEffect(() => {
    // fire only when token transitions falsy -> truthy (login event)
    const tokenJustBecameValid = !lastToken.current && !!token;
    lastToken.current = token;

    if (!isAllowed || !tokenJustBecameValid) return;

    let ignore = false;

    (async () => {
      try {
        const res = await http("/api/replenishment/suggestions");
        // support either raw array or axios-like { data: [...] }
        const data = Array.isArray(res) ? res : res?.data ?? [];
        if (!ignore && Array.isArray(data) && data.length > 0) {
          setRows(data);
          setVisible(true);
        }
      } catch {
        // silently ignore
      }
    })();

    return () => {
      ignore = true;
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [token, isAllowed]);

  if (!isAllowed || !visible) return null;

  return (
    <div style={backdrop}>
      <div style={modal}>
        <h3 style={{ marginTop: 0 }}>Low stock items need attention</h3>

        <div style={{ maxHeight: 320, overflow: "auto", border: "1px solid #eee" }}>
          <table style={{ width: "100%", borderCollapse: "collapse" }}>
            <thead>
              <tr>
                <th style={th}>Office</th>
                <th style={th}>Item</th>
                <th style={{ ...th, textAlign: "right" }}>Qty</th>
                <th style={{ ...th, textAlign: "right" }}>Threshold</th>
                <th style={th}>Supplier</th>
              </tr>
            </thead>
            <tbody>
              {rows.map((r) => (
                <tr key={r.stockLevelId ?? `${r.officeName}-${r.itemName}`}>
                  <td style={td}>{r.officeName}</td>
                  <td style={td}>{r.itemName}</td>
                  <td style={{ ...td, textAlign: "right" }}>{r.quantity}</td>
                  <td style={{ ...td, textAlign: "right" }}>{r.reorderThreshold}</td>
                  <td style={td}>{r.supplierName ?? "-"}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        <div style={{ display: "flex", gap: 8, marginTop: 12, justifyContent: "flex-end" }}>
          <a href="/replenishment" style={{ textDecoration: "none" }}>
            <button>Open Replenishment</button>
          </a>
          <button onClick={() => setVisible(false)}>Dismiss</button>
        </div>
      </div>
    </div>
  );
}

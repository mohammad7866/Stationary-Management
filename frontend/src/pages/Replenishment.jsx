import React, { useEffect, useMemo, useState } from "react";
import { Replenishment, Offices } from "../lib/api";
import { useAuth } from "../auth/AuthContext";
import { can } from "../auth/permissions";

export default function ReplenishmentPage() {
  const { roles = [] } = useAuth();
  const canRaise = can(roles, "DeliveriesCreate");

  const [office, setOffice] = useState("All");
  const [offices, setOffices] = useState([]);
  const [rows, setRows] = useState([]);
  const [selected, setSelected] = useState({}); // stockLevelId -> boolean
  const [loading, setLoading] = useState(false);
  const [msg, setMsg] = useState("");

  useEffect(() => {
    (async () => {
      try {
        const o = await Offices.list();
        setOffices(Array.isArray(o) ? o : o?.data ?? []);
      } catch (e) {
        setMsg(e.message || "Failed to load offices");
      }
    })();
  }, []);

  async function load() {
    setLoading(true);
    setMsg("");
    try {
      const params = {};
      if (office !== "All") params.office = office;
      const data = await Replenishment.suggestions(params);
      setRows(Array.isArray(data) ? data : data?.data ?? []);
      setSelected({});
    } catch (e) {
      setMsg(e.message || "Failed to load suggestions");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => { load(); /* eslint-disable-next-line */ }, [office]);

  const totalSelected = useMemo(
    () => Object.values(selected).filter(Boolean).length,
    [selected]
  );

  async function raise() {
    const lines = rows
      .filter(r => selected[r.stockLevelId])
      .map(r => ({
        stockLevelId: r.stockLevelId,
        itemId: r.itemId,
        officeId: r.officeId,
        supplierId: r.supplierId,
        quantity: r.suggestedOrderQty,
      }));

    if (lines.length === 0) {
      setMsg("Select at least one line.");
      return;
    }
    try {
      const res = await Replenishment.raise({ lines });
      setMsg(`Raised ${res.created} delivery orders.`);
      setSelected({});
    } catch (e) {
      setMsg(e.message || "Failed to raise deliveries");
    }
  }

  return (
    <div style={{ padding: 20, maxWidth: 1000, margin: "0 auto" }}>
      <h1 style={{ fontSize: 24, fontWeight: 700 }}>Replenishment</h1>

      <div style={{ display: "flex", gap: 12, alignItems: "center", margin: "12px 0" }}>
        <label>Office:</label>
        <select value={office} onChange={(e) => setOffice(e.target.value)}>
          <option>All</option>
          {offices.map((o) => <option key={o.id}>{o.name}</option>)}
        </select>
        <button onClick={load} style={btn}>Refresh</button>
      </div>

      {msg && <div style={{ marginBottom: 10 }}>{msg}</div>}
      {loading && <div style={{ color: "#666", marginBottom: 10 }}>Loading…</div>}

      <table style={{ width: "100%", borderCollapse: "collapse" }}>
        <thead>
          <tr style={{ background: "#f0f0f0" }}>
            <th style={th}>Select</th>
            <th style={th}>Item</th>
            <th style={th}>Office</th>
            <th style={th}>Qty</th>
            <th style={th}>Threshold</th>
            <th style={th}>Shortage</th>
            <th style={th}>Suggested Order</th>
            <th style={th}>Supplier</th>
          </tr>
        </thead>
        <tbody>
          {rows.map((r) => (
            <tr key={r.stockLevelId}>
              <td style={td}>
                <input
                  type="checkbox"
                  checked={!!selected[r.stockLevelId]}
                  onChange={(e) => setSelected((s) => ({ ...s, [r.stockLevelId]: e.target.checked }))}
                />
              </td>
              <td style={td}>{r.itemName}</td>
              <td style={td}>{r.officeName}</td>
              <td style={td}>{r.quantity}</td>
              <td style={td}>{r.reorderThreshold}</td>
              <td style={td}>{Math.max(0, r.reorderThreshold - r.quantity)}</td>
              <td style={td}><b>{r.suggestedOrderQty}</b></td>
              <td style={td}>{r.supplierName || "-"}</td>
            </tr>
          ))}
          {rows.length === 0 && (
            <tr><td style={td} colSpan={8}>No low-stock items 🎉</td></tr>
          )}
        </tbody>
      </table>

      {canRaise && (
        <div style={{ marginTop: 12, display: "flex", gap: 8 }}>
          <button disabled={totalSelected === 0} onClick={raise} style={btn}>
            Raise Deliveries ({totalSelected})
          </button>
        </div>
      )}
    </div>
  );
}

const th = { padding: 12, borderBottom: "1px solid #ccc", textAlign: "left" };
const td = { padding: 10, borderBottom: "1px solid #eee" };
const btn = { padding: "8px 12px", background: "black", color: "white", border: 0, borderRadius: 6 };

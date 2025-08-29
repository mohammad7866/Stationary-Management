// src/pages/Dashboard.jsx
// Simple dashboard pulling live data from the API
import React, { useEffect, useMemo, useState } from "react";
import { http } from "../lib/api"; // uses Authorization header automatically

export default function Dashboard() {
  const [items, setItems] = useState([]);
  const [stock, setStock] = useState([]);
  const [requests, setRequests] = useState([]);
  const [deliveries, setDeliveries] = useState([]);
  const [offices, setOffices] = useState([]);
  const [errorMsg, setErrorMsg] = useState("");

  // normalize API responses (array or { data: [...] })
  const arr = (x) => (Array.isArray(x) ? x : x?.data ?? []);

  useEffect(() => {
    (async () => {
      try {
        const [iRes, sRes, rRes, dRes, oRes] = await Promise.all([
          http(`/api/Items?page=1&pageSize=500`),
          http(`/api/StockLevels`),
          http(`/api/Requests`),
          http(`/api/Deliveries`),
          http(`/api/Offices`),
        ]);
        setItems(arr(iRes));
        setStock(arr(sRes));
        setRequests(arr(rRes));
        setDeliveries(arr(dRes));
        setOffices(arr(oRes));
      } catch (e) {
        setErrorMsg(e.message || "Failed to load dashboard data.");
      }
    })();
  }, []);

  const totals = useMemo(() => {
    const totalItems = items.length;
    const totalStock = stock.reduce((acc, s) => acc + (s.quantity ?? 0), 0);

    const requestsPending = requests.filter(r => r.status === "Pending").length;
    const deliveriesPending = deliveries.filter(d => d.status === "Pending").length;

    // Overdue deliveries: expected date < today and not Received/Cancelled
    const todayYmd = new Date().toISOString().slice(0, 10);
    const overdueDeliveries = deliveries.filter(d => {
      const expected = d.expectedArrivalDateUtc;
      if (!expected) return false;
      const ymd = new Date(expected).toISOString().slice(0, 10);
      return ymd < todayYmd && d.status !== "Received" && d.status !== "Cancelled";
    }).length;

    // Low stock (client-side): quantity <= reorderThreshold (when threshold > 0)
    const lowStockItems = stock.filter(s =>
      (s.reorderThreshold ?? 0) > 0 && (s.quantity ?? 0) <= (s.reorderThreshold ?? 0)
    ).length;

    // Stock by office (prefer joined Office name)
    const stockByOffice = new Map();
    for (const s of stock) {
      const name =
        s.office?.name ??
        offices.find(o => o.id === s.officeId)?.name ??
        `Office ${s.officeId}`;
      stockByOffice.set(name, (stockByOffice.get(name) || 0) + (s.quantity ?? 0));
    }

    // Requests by status
    const reqByStatus = new Map();
    for (const r of requests) {
      const k = r.status || "Unknown";
      reqByStatus.set(k, (reqByStatus.get(k) || 0) + 1);
    }

    // Deliveries by status
    const delByStatus = new Map();
    for (const d of deliveries) {
      const k = d.status || "Unknown";
      delByStatus.set(k, (delByStatus.get(k) || 0) + 1);
    }

    return {
      totalItems,
      totalStock,
      requestsPending,
      deliveriesPending,
      overdueDeliveries,
      lowStockItems,
      stockByOffice,
      reqByStatus,
      delByStatus
    };
  }, [items, stock, requests, deliveries, offices]);

  return (
    <div style={container}>
      <h1 style={title}>Dashboard</h1>
      {errorMsg && <div style={errorStyle}>{errorMsg}</div>}

      <div style={cardsRow}>
        <div style={card}><div style={kpiTitle}>Total Items</div><div style={kpiValue}>{totals.totalItems}</div></div>
        <div style={card}><div style={kpiTitle}>Total Stock (all offices)</div><div style={kpiValue}>{totals.totalStock}</div></div>
        <div style={card}><div style={kpiTitle}>Requests Pending</div><div style={kpiValue}>{totals.requestsPending}</div></div>
        <div style={card}><div style={kpiTitle}>Deliveries Pending</div><div style={kpiValue}>{totals.deliveriesPending}</div></div>
        <div style={{...card, borderColor:"#ffc107"}}>
          <div style={kpiTitle}>Overdue Deliveries</div>
          <div style={{...kpiValue, color: totals.overdueDeliveries ? "#dc3545" : "#198754"}}>
            {totals.overdueDeliveries}
          </div>
        </div>
        <div style={{...card, borderColor:"#dc3545"}}>
          <div style={kpiTitle}>Low-Stock Items</div>
          <div style={{...kpiValue, color: totals.lowStockItems ? "#dc3545" : "#198754"}}>
            {totals.lowStockItems}
          </div>
        </div>
      </div>

      <div style={grid}>
        <div style={panel}>
          <h2>Stock by Office</h2>
          <table style={{ width: "100%", borderCollapse: "collapse" }}>
            <thead>
              <tr style={{ background: "#f0f0f0" }}>
                <th style={thStyle}>Office</th>
                <th style={thStyle}>Quantity</th>
              </tr>
            </thead>
            <tbody>
              {Array.from(totals.stockByOffice.entries()).map(([office, qty]) => (
                <tr key={office}>
                  <td style={tdStyle}>{office}</td>
                  <td style={tdStyle}>{qty}</td>
                </tr>
              ))}
              {totals.stockByOffice.size === 0 && (
                <tr><td style={tdStyle} colSpan={2}>No stock data</td></tr>
              )}
            </tbody>
          </table>
        </div>

        <div style={panel}>
          <h2>Requests by Status</h2>
          <table style={{ width: "100%", borderCollapse: "collapse" }}>
            <thead>
              <tr style={{ background: "#f0f0f0" }}>
                <th style={thStyle}>Status</th>
                <th style={thStyle}>Count</th>
              </tr>
            </thead>
            <tbody>
              {Array.from(totals.reqByStatus.entries()).map(([status, count]) => (
                <tr key={status}>
                  <td style={tdStyle}>{status}</td>
                  <td style={tdStyle}>{count}</td>
                </tr>
              ))}
              {totals.reqByStatus.size === 0 && (
                <tr><td style={tdStyle} colSpan={2}>No requests</td></tr>
              )}
            </tbody>
          </table>
        </div>

        <div style={panel}>
          <h2>Deliveries by Status</h2>
          <table style={{ width: "100%", borderCollapse: "collapse" }}>
            <thead>
              <tr style={{ background: "#f0f0f0" }}>
                <th style={thStyle}>Status</th>
                <th style={thStyle}>Count</th>
              </tr>
            </thead>
            <tbody>
              {Array.from(totals.delByStatus.entries()).map(([status, count]) => (
                <tr key={status}>
                  <td style={tdStyle}>{status}</td>
                  <td style={tdStyle}>{count}</td>
                </tr>
              ))}
              {totals.delByStatus.size === 0 && (
                <tr><td style={tdStyle} colSpan={2}>No deliveries</td></tr>
              )}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}

/* styles */
const container = { padding: "20px", maxWidth: "1100px", margin: "0 auto" };
const title = { fontSize: "24px", fontWeight: "bold", marginBottom: "10px" };
const errorStyle = { color: "crimson", marginBottom: "10px" };

const cardsRow = {
  display: "grid",
  gridTemplateColumns: "repeat(4, 1fr)",
  gap: 12,
  marginBottom: 16
};
// responsive: add two extra KPI cards onto a new row on narrow screens
if (typeof window !== "undefined" && window.matchMedia && window.matchMedia("(max-width: 900px)").matches) {
  cardsRow.gridTemplateColumns = "repeat(2, 1fr)";
}

const card = { border: "1px solid #eee", borderRadius: 8, padding: 12, background: "#fff" };
const kpiTitle = { fontSize: 12, color: "#666" };
const kpiValue = { fontSize: 22, fontWeight: 700 };

const grid = { display: "grid", gridTemplateColumns: "1fr 1fr", gap: 16 };
const panel = { border: "1px solid #eee", borderRadius: 8, padding: 12, background: "#fff" };
const thStyle = { padding: "10px", borderBottom: "1px solid #ccc", textAlign: "left" };
const tdStyle = { padding: "8px", borderBottom: "1px solid #eee" };

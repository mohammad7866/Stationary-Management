// Simple dashboard pulling live data from the API
import React, { useEffect, useMemo, useState } from "react";
import { Items, StockLevels, Requests, Deliveries, Offices } from "../lib/api";

export default function Dashboard() {
  const [items, setItems] = useState([]);
  const [stock, setStock] = useState([]);
  const [requests, setRequests] = useState([]);
  const [deliveries, setDeliveries] = useState([]);
  const [offices, setOffices] = useState([]);
  const [errorMsg, setErrorMsg] = useState("");

  useEffect(() => {
    (async () => {
      try {
        const [iRes, sRes, rRes, dRes, oRes] = await Promise.all([
          Items.list({ page: 1, pageSize: 500 }),
          StockLevels.list(),
          Requests.list(),
          Deliveries.list(),
          Offices.list(),
        ]);
        setItems(iRes?.data ?? iRes ?? []);
        setStock(Array.isArray(sRes) ? sRes : sRes?.data ?? []);
        setRequests(Array.isArray(rRes) ? rRes : rRes?.data ?? []);
        setDeliveries(Array.isArray(dRes) ? dRes : dRes?.data ?? []);
        setOffices(Array.isArray(oRes) ? oRes : oRes?.data ?? []);
      } catch (e) {
        setErrorMsg(e.message);
      }
    })();
  }, []);

  const totals = useMemo(() => {
    const totalItems = items.length;
    const totalStock = stock.reduce((acc, s) => acc + (s.quantity || 0), 0);
    const requestsPending = requests.filter(r => r.status === "Pending").length;
    const deliveriesPending = deliveries.filter(d => d.status === "Pending").length;

    // per office stock
    const officeById = new Map(offices.map(o => [o.id, o.name]));
    const stockByOffice = new Map();
    for (const s of stock) {
      const name = officeById.get(s.officeId) ?? `Office ${s.officeId}`;
      stockByOffice.set(name, (stockByOffice.get(name) || 0) + (s.quantity || 0));
    }

    // requests by status
    const reqByStatus = new Map();
    for (const r of requests) {
      const k = r.status || "Unknown";
      reqByStatus.set(k, (reqByStatus.get(k) || 0) + 1);
    }

    // deliveries by status
    const delByStatus = new Map();
    for (const d of deliveries) {
      const k = d.status || "Unknown";
      delByStatus.set(k, (delByStatus.get(k) || 0) + 1);
    }

    return { totalItems, totalStock, requestsPending, deliveriesPending, stockByOffice, reqByStatus, delByStatus };
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
      </div>

      <div style={grid}>
        <div style={panel}>
          <h2>Stock by Office</h2>
          <table style={{ width: "100%", borderCollapse: "collapse" }}>
            <thead><tr style={{ background: "#f0f0f0" }}><th style={thStyle}>Office</th><th style={thStyle}>Quantity</th></tr></thead>
            <tbody>
              {Array.from(totals.stockByOffice.entries()).map(([office, qty]) => (
                <tr key={office}><td style={tdStyle}>{office}</td><td style={tdStyle}>{qty}</td></tr>
              ))}
              {totals.stockByOffice.size === 0 && <tr><td style={tdStyle} colSpan={2}>No stock data</td></tr>}
            </tbody>
          </table>
        </div>

        <div style={panel}>
          <h2>Requests by Status</h2>
          <table style={{ width: "100%", borderCollapse: "collapse" }}>
            <thead><tr style={{ background: "#f0f0f0" }}><th style={thStyle}>Status</th><th style={thStyle}>Count</th></tr></thead>
            <tbody>
              {Array.from(totals.reqByStatus.entries()).map(([status, count]) => (
                <tr key={status}><td style={tdStyle}>{status}</td><td style={tdStyle}>{count}</td></tr>
              ))}
              {totals.reqByStatus.size === 0 && <tr><td style={tdStyle} colSpan={2}>No requests</td></tr>}
            </tbody>
          </table>
        </div>

        <div style={panel}>
          <h2>Deliveries by Status</h2>
          <table style={{ width: "100%", borderCollapse: "collapse" }}>
            <thead><tr style={{ background: "#f0f0f0" }}><th style={thStyle}>Status</th><th style={thStyle}>Count</th></tr></thead>
            <tbody>
              {Array.from(totals.delByStatus.entries()).map(([status, count]) => (
                <tr key={status}><td style={tdStyle}>{status}</td><td style={tdStyle}>{count}</td></tr>
              ))}
              {totals.delByStatus.size === 0 && <tr><td style={tdStyle} colSpan={2}>No deliveries</td></tr>}
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
const cardsRow = { display: "grid", gridTemplateColumns: "repeat(4, 1fr)", gap: 12, marginBottom: 16 };
const card = { border: "1px solid #eee", borderRadius: 8, padding: 12, background: "#fff" };
const kpiTitle = { fontSize: 12, color: "#666" };
const kpiValue = { fontSize: 22, fontWeight: 700 };
const grid = { display: "grid", gridTemplateColumns: "1fr 1fr", gap: 16 };
const panel = { border: "1px solid #eee", borderRadius: 8, padding: 12, background: "#fff" };
const thStyle = { padding: "10px", borderBottom: "1px solid #ccc", textAlign: "left" };
const tdStyle = { padding: "8px", borderBottom: "1px solid #eee" };

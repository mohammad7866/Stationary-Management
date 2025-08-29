// frontend/src/pages/Deliveries.jsx
import React, { useEffect, useMemo, useState } from "react";
import { Deliveries, Items, Offices, Suppliers } from "../lib/api";

// yyyy-mm-dd from ISO
function fmtIsoDate(iso) {
  if (!iso) return "";
  if (String(iso).startsWith("0001-")) return "";
  const d = new Date(iso);
  if (isNaN(d.getTime())) return String(iso);
  return d.toISOString().slice(0, 10);
}

// UTC midnight ISO from yyyy-mm-dd
function toUtcMidnightIso(dateStr) {
  if (!dateStr) return null;
  return new Date(`${dateStr}T00:00:00Z`).toISOString();
}

const isNeg = (v) => typeof v === "number" && v < 0;

// Overdue = expected date past and not received/cancelled
const todayYmd = new Date().toISOString().slice(0, 10);
const isOverdueRow = (r) =>
  r.status !== "Received" &&
  r.status !== "Cancelled" &&
  r.arrival &&
  r.arrival < todayYmd;

const overdueRowStyle = {
  background: "#ffe6e6",
  boxShadow: "inset 4px 0 0 #dc3545"
};

export default function DeliveriesPage() {
  const [list, setList] = useState([]);
  const [items, setItems] = useState([]);
  const [offices, setOffices] = useState([]);
  const [suppliers, setSuppliers] = useState([]);
  const [statusMsg, setStatusMsg] = useState("");
  const [errorMsg, setErrorMsg] = useState("");

  const [filterOffice, setFilterOffice] = useState("All");
  const [filterStatus, setFilterStatus] = useState("All");

  // Create form uses order & expected; actual is set when marking Received
  const [form, setForm] = useState({
    product: "",
    office: "",
    supplierId: "",
    orderedDate: "",          // yyyy-mm-dd
    expectedArrivalDate: "",  // yyyy-mm-dd
    status: "Pending",
  });

  const rows = useMemo(() => {
    return (list || [])
      .map(r => ({
        ...r,
        scheduled: fmtIsoDate(r.orderedDateUtc),
        arrival:   fmtIsoDate(r.expectedArrivalDateUtc),
        actual:    fmtIsoDate(r.actualArrivalDateUtc),
      }))
      .filter(r =>
        (filterOffice === "All" || r.office === filterOffice) &&
        (filterStatus === "All" || r.status === filterStatus)
      )
      .sort((a, b) => (a.scheduled || "").localeCompare(b.scheduled || ""));
  }, [list, filterOffice, filterStatus]);

  async function load() {
    try {
      const [dRes, iRes, oRes, sRes] = await Promise.all([
        Deliveries.list(),
        Items.list({ page: 1, pageSize: 500 }),
        Offices.list(),
        Suppliers.list({ page: 1, pageSize: 500 }),
      ]);
      setList(Array.isArray(dRes) ? dRes : dRes?.data ?? []);
      setItems(iRes?.data ?? iRes ?? []);
      setOffices(Array.isArray(oRes) ? oRes : oRes?.data ?? []);
      setSuppliers(sRes?.data ?? []);
    } catch (e) { setErrorMsg(e.message); }
  }
  useEffect(() => { load(); }, []);

  async function createDelivery(e) {
    e.preventDefault();
    setStatusMsg(""); setErrorMsg("");
    try {
      const orderedIso = toUtcMidnightIso(form.orderedDate);
      const expectedIso = toUtcMidnightIso(form.expectedArrivalDate);

      if (!form.product.trim()) throw new Error("Product is required.");
      if (!form.office) throw new Error("Office is required.");
      if (!orderedIso) throw new Error("Order date is required.");
      if (!expectedIso) throw new Error("Expected arrival date is required.");
      if (new Date(expectedIso) < new Date(orderedIso)) {
        throw new Error("Expected arrival cannot be before the order date.");
      }

      const payload = {
        product: form.product.trim(),
        office: form.office,
        supplierId: form.supplierId ? Number(form.supplierId) : null,
        orderedDateUtc: orderedIso,
        expectedArrivalDateUtc: expectedIso,
        status: form.status || "Pending",
      };

      await Deliveries.create(payload);
      setForm({ product: "", office: "", supplierId: "", orderedDate: "", expectedArrivalDate: "", status: "Pending" });
      await load();
      setStatusMsg("Delivery created.");
    } catch (e) { setErrorMsg(e.message); }
  }

  async function updateStatus(id, status) {
    setStatusMsg(""); setErrorMsg("");
    try {
      if (status === "Received") {
        const defaultVal = new Date().toISOString().slice(0,10);
        const dateStr = window.prompt("Enter ACTUAL arrival date (YYYY-MM-DD):", defaultVal);
        if (!dateStr) return;
        if (!/^\d{4}-\d{2}-\d{2}$/.test(dateStr)) {
          throw new Error("Please enter the arrival date as YYYY-MM-DD.");
        }
        await Deliveries.update(id, {
          status: "Received",
          actualArrivalDateUtc: toUtcMidnightIso(dateStr),
        });
      } else {
        await Deliveries.updateStatus(id, { status });
      }
      await load();
      setStatusMsg(`Delivery ${status}.`);
    } catch (e) { setErrorMsg(e.message); }
  }

  async function deleteDelivery(id) {
    if (!confirm("Delete this delivery?")) return;
    setStatusMsg(""); setErrorMsg("");
    try {
      await Deliveries.remove(id);
      await load();
      setStatusMsg("Delivery deleted.");
    } catch (e) { setErrorMsg(e.message); }
  }

  return (
    <div style={container}>
      <h1 style={title}>Deliveries</h1>

      <div style={filtersRow}>
        <div>
          <label>Office: </label>
          <select value={filterOffice} onChange={(e)=>setFilterOffice(e.target.value)}>
            <option>All</option>
            {offices.map(o => <option key={o.id}>{o.name}</option>)}
          </select>
        </div>
        <div>
          <label>Status: </label>
          <select value={filterStatus} onChange={(e)=>setFilterStatus(e.target.value)}>
            <option>All</option>
            <option>Pending</option>
            <option>On Time</option>
            <option>Delayed</option>
            <option>Received</option>
            <option>Cancelled</option>
          </select>
        </div>
      </div>

      {statusMsg && <div style={successStyle}>{statusMsg}</div>}
      {errorMsg && <div style={errorStyle}>{errorMsg}</div>}

      <table style={{ width: "100%", borderCollapse: "collapse" }}>
        <thead>
          <tr style={{ background: "#f0f0f0" }}>
            <th style={thStyle}>Product</th>
            <th style={thStyle}>Supplier</th>
            <th style={thStyle}>Office</th>
            <th style={thStyle}>Scheduled</th>
            <th style={thStyle}>Arrival</th>
            <th style={thStyle}>Delay (days)</th>
            <th style={thStyle}>Status</th>
            <th style={thStyle}>Actions</th>
          </tr>
        </thead>
        <tbody>
          {rows.map(r => (
            <tr key={r.id} style={isOverdueRow(r) ? overdueRowStyle : undefined}>
              <td style={tdStyle}>{r.product}</td>
              <td style={tdStyle}>{r.supplierName || "-"}</td>
              <td style={tdStyle}>{r.office}</td>
              <td style={tdStyle}>{r.scheduled || "-"}</td>
              <td style={tdStyle}>{r.arrival || "-"}</td>
              <td
                style={{
                  ...tdStyle,
                  color: isNeg(r.arrivalDelayDays) ? "#dc3545" : "inherit",
                  fontWeight: isNeg(r.arrivalDelayDays) ? 600 : 400
                }}
              >
                {r.arrivalDelayDays ?? "-"}
              </td>
              <td style={tdStyle}>{r.status}</td>
              <td style={tdStyle}>
                <button style={button} onClick={()=>updateStatus(r.id, "On Time")}>On Time</button>
                <button style={button} onClick={()=>updateStatus(r.id, "Delayed")}>Delayed</button>
                <button style={button} onClick={()=>updateStatus(r.id, "Received")}>Received</button>
                <button style={dangerButton} onClick={()=>deleteDelivery(r.id)}>Delete</button>
              </td>
            </tr>
          ))}
          {rows.length === 0 && <tr><td style={tdStyle} colSpan={8}>No deliveries</td></tr>}
        </tbody>
      </table>

      <form onSubmit={createDelivery} style={formStyle}>
        <h2>Schedule Delivery</h2>

        <div style={formRow}>
          <label style={{ width: 140 }}>Product</label>
          <select
            value={form.product}
            onChange={(e)=>setForm(f=>({...f, product: e.target.value}))}
            required
            style={select}
          >
            <option value="">Select</option>
            {items.map(i => <option key={i.id} value={i.name}>{i.name}</option>)}
          </select>
        </div>

        <div style={formRow}>
          <label style={{ width: 140 }}>Supplier</label>
          <select
            value={form.supplierId}
            onChange={(e)=>setForm(f=>({...f, supplierId: e.target.value}))}
            style={select}
          >
            <option value="">None</option>
            {suppliers.map(s => <option key={s.id} value={s.id}>{s.name}</option>)}
          </select>
        </div>

        <div style={formRow}>
          <label style={{ width: 140 }}>Office</label>
          <select
            value={form.office}
            onChange={(e)=>setForm(f=>({...f, office: e.target.value}))}
            required
            style={select}
          >
            <option value="">Select</option>
            {offices.map(o => <option key={o.id} value={o.name}>{o.name}</option>)}
          </select>
        </div>

        <div style={formRow}>
          <label style={{ width: 140 }}>Order Date</label>
          <input
            type="date"
            value={form.orderedDate}
            onChange={(e)=>setForm(f=>({...f, orderedDate: e.target.value}))}
            required
            style={input}
          />
        </div>

        <div style={formRow}>
          <label style={{ width: 140 }}>Expected Arrival</label>
          <input
            type="date"
            value={form.expectedArrivalDate}
            onChange={(e)=>setForm(f=>({...f, expectedArrivalDate: e.target.value}))}
            required
            style={input}
          />
        </div>

        <div style={formRow}>
          <label style={{ width: 140 }}>Status</label>
          <select
            value={form.status}
            onChange={(e)=>setForm(f=>({...f, status: e.target.value}))}
            style={select}
          >
            <option>Pending</option>
            <option>On Time</option>
            <option>Delayed</option>
            <option>Received</option>
            <option>Cancelled</option>
          </select>
        </div>

        <button style={button}>Create</button>
      </form>
    </div>
  );
}

/* styles */
const container = { padding: "20px", maxWidth: "1000px", margin: "0 auto" };
const title = { fontSize: "24px", fontWeight: "bold", marginBottom: "10px" };
const filtersRow = { display: "flex", gap: "15px", margin: "15px 0" };
const formStyle = { marginTop: "20px", padding: "15px", border: "1px solid #ddd", borderRadius: "8px" };
const formRow = { display: "flex", alignItems: "center", gap: "10px", marginBottom: "10px" };
const input = { padding: "8px", border: "1px solid #ccc", borderRadius: "4px", width: "100%" };
const select = { padding: "8px", border: "1px solid #ccc", borderRadius: "4px" };
const button = { padding: "8px 12px", background: "#007bff", color: "#fff", border: "none", borderRadius: "4px", cursor: "pointer", marginRight: "8px" };
const dangerButton = { ...button, background: "#dc3545" };
const successStyle = { color: "green", marginBottom: "10px" };
const errorStyle = { color: "crimson", marginBottom: "10px" };
const thStyle = { padding: "12px", borderBottom: "1px solid #ccc", textAlign: "left" };
const tdStyle = { padding: "10px", borderBottom: "1px solid #eee" };

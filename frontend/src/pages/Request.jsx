// frontend/src/pages/Requests.jsx
import React, { useEffect, useMemo, useState } from "react";
import { Requests, Items, Offices } from "../lib/api";
import { useAuth } from "../auth/AuthContext";

// yyyy-mm-dd
function fmtIsoDate(iso) {
  if (!iso) return "";
  if (String(iso).startsWith("0001-")) return "";
  const d = new Date(iso);
  if (isNaN(d.getTime())) return "";
  return d.toISOString().slice(0, 10);
}

// yyyy-mm-dd HH:mm (local)
function fmtIsoDateTime(iso) {
  if (!iso) return "";
  const d = new Date(iso);
  if (isNaN(d.getTime())) return "";
  const y = d.getFullYear();
  const m = String(d.getMonth() + 1).padStart(2, "0");
  const day = String(d.getDate()).padStart(2, "0");
  const hh = String(d.getHours()).padStart(2, "0");
  const mm = String(d.getMinutes()).padStart(2, "0");
  return `${y}-${m}-${day} ${hh}:${mm}`;
}

const statusColors = {
  Pending: { bg: "#f1f1f1", fg: "#333" },
  Approved: { bg: "#e6f4ea", fg: "#1e7e34" },
  Rejected: { bg: "#ffe6e6", fg: "#dc3545" },
};

export default function RequestsPage() {
  const { roles } = useAuth();
  const canModerate = roles.includes("Admin") || roles.includes("SuperAdmin");
  const canDelete = roles.includes("SuperAdmin");

  const [list, setList] = useState([]);
  const [items, setItems] = useState([]);
  const [offices, setOffices] = useState([]);

  const [statusMsg, setStatusMsg] = useState("");
  const [errorMsg, setErrorMsg] = useState("");
  const [loading, setLoading] = useState(false);
  const [busyId, setBusyId] = useState(null);

  const [filterOffice, setFilterOffice] = useState("All");
  const [filterStatus, setFilterStatus] = useState("All");
  const [search, setSearch] = useState("");

  const [form, setForm] = useState({
    itemName: "",
    office: "",
    quantity: "",
    purpose: "",
    status: "Pending",
  });

  const rows = useMemo(() => {
    const q = (search || "").toLowerCase().trim();
    return (list || [])
      .filter(r =>
        (filterOffice === "All" || r.office === filterOffice) &&
        (filterStatus === "All" || r.status === filterStatus) &&
        (!q ||
          r.itemName?.toLowerCase().includes(q) ||
          r.office?.toLowerCase().includes(q) ||
          String(r.quantity).includes(q) ||
          (r.purpose || "").toLowerCase().includes(q))
      )
      .sort((a, b) => {
        const ad = a.createdAt ? new Date(a.createdAt).getTime() : 0;
        const bd = b.createdAt ? new Date(b.createdAt).getTime() : 0;
        if (ad !== bd) return bd - ad;
        return (b.id || 0) - (a.id || 0);
      });
  }, [list, filterOffice, filterStatus, search]);

  async function load() {
    setLoading(true);
    setErrorMsg("");
    try {
      const [rRes, iRes, oRes] = await Promise.all([
        Requests.list(),
        Items.list({ page: 1, pageSize: 500 }),
        Offices.list(),
      ]);
      setList(Array.isArray(rRes) ? rRes : rRes?.data ?? []);
      setItems(iRes?.data ?? iRes ?? []);
      setOffices(Array.isArray(oRes) ? oRes : oRes?.data ?? []);
    } catch (e) {
      setErrorMsg(e.message || "Failed to load requests.");
    } finally {
      setLoading(false);
    }
  }
  useEffect(() => { load(); }, []);

  async function createRequest(e) {
    e.preventDefault();
    setStatusMsg(""); setErrorMsg("");
    try {
      if (!form.itemName) throw new Error("Item is required.");
      if (!form.office) throw new Error("Office is required.");
      const qty = parseInt(String(form.quantity), 10);
      if (!Number.isFinite(qty) || qty <= 0) throw new Error("Quantity must be a positive number.");

      const payload = {
        itemName: form.itemName,
        office: form.office,
        quantity: qty,
        purpose: form.purpose || "",
        status: form.status || "Pending",
      };

      await Requests.create(payload);
      setForm({ itemName: "", office: "", quantity: "", purpose: "", status: "Pending" });
      await load();
      setStatusMsg("Request submitted.");
    } catch (e) { setErrorMsg(e.message); }
  }

  async function onApprove(id) {
    setBusyId(id); setStatusMsg(""); setErrorMsg("");
    try {
      await Requests.approve(id);
      await load();
      setStatusMsg("Request approved.");
    } catch (e) { setErrorMsg(e.message); }
    finally { setBusyId(null); }
  }

  async function onReject(id) {
    setBusyId(id); setStatusMsg(""); setErrorMsg("");
    try {
      await Requests.reject(id);
      await load();
      setStatusMsg("Request rejected.");
    } catch (e) { setErrorMsg(e.message); }
    finally { setBusyId(null); }
  }

  async function onDelete(id) {
    if (!confirm("Delete this request?")) return;
    setBusyId(id); setStatusMsg(""); setErrorMsg("");
    try {
      await Requests.remove(id);
      await load();
      setStatusMsg("Request deleted.");
    } catch (e) { setErrorMsg(e.message); }
    finally { setBusyId(null); }
  }

  return (
    <div style={container}>
      <h1 style={title}>Requests</h1>

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
            <option>Approved</option>
            <option>Rejected</option>
          </select>
        </div>
        <div style={{ flex: 1 }} />
        <input
          placeholder="Search item, office, purpose..."
          value={search}
          onChange={(e)=>setSearch(e.target.value)}
          style={{ padding: 8, border: "1px solid #ccc", borderRadius: 4, minWidth: 240 }}
        />
      </div>

      {statusMsg && <div style={successStyle}>{statusMsg}</div>}
      {errorMsg && <div style={errorStyle}>{errorMsg}</div>}
      {loading && <div style={{ color:"#666", marginBottom:10 }}>Loading…</div>}

      <table style={{ width: "100%", borderCollapse: "collapse" }}>
        <thead>
          <tr style={{ background: "#f0f0f0" }}>
            <th style={thStyle}>Item</th>
            <th style={thStyle}>Office</th>
            <th style={thStyle}>Qty</th>
            <th style={thStyle}>Purpose</th>
            <th style={thStyle}>Created</th>
            <th style={thStyle}>Decision</th>
            <th style={thStyle}>Status</th>
            <th style={thStyle}>Actions</th>
          </tr>
        </thead>
        <tbody>
          {rows.map(r => {
            const colors = statusColors[r.status] || statusColors.Pending;
            return (
              <tr key={r.id}>
                <td style={tdStyle}>{r.itemName}</td>
                <td style={tdStyle}>{r.office}</td>
                <td style={tdStyle}>{r.quantity}</td>
                <td style={tdStyle}>{r.purpose || "-"}</td>
                <td style={tdStyle}>{fmtIsoDateTime(r.createdAt)}</td>
                <td style={tdStyle}>{r.decisionAt ? fmtIsoDateTime(r.decisionAt) : "-"}</td>
                <td style={{ ...tdStyle }}>
                  <span style={{
                    padding: "2px 8px",
                    borderRadius: 999,
                    background: colors.bg,
                    color: colors.fg,
                    fontWeight: 600,
                    fontSize: 12
                  }}>{r.status}</span>
                </td>
                <td style={tdStyle}>
                  {canModerate && r.status === "Pending" && (
                    <>
                      <button
                        style={button}
                        disabled={busyId === r.id}
                        onClick={()=>onApprove(r.id)}
                      >Approve</button>
                      <button
                        style={dangerButton}
                        disabled={busyId === r.id}
                        onClick={()=>onReject(r.id)}
                      >Reject</button>
                    </>
                  )}
                  {canDelete && (
                    <button
                      style={ghostButton}
                      disabled={busyId === r.id}
                      onClick={()=>onDelete(r.id)}
                      title="Delete (SuperAdmin)"
                    >Delete</button>
                  )}
                </td>
              </tr>
            );
          })}
          {rows.length === 0 && (
            <tr><td style={tdStyle} colSpan={8}>No requests</td></tr>
          )}
        </tbody>
      </table>

      <form onSubmit={createRequest} style={formStyle}>
        <h2>New Request</h2>

        <div style={formRow}>
          <label style={{ width: 140 }}>Item</label>
          <select
            value={form.itemName}
            onChange={(e)=>setForm(f=>({...f, itemName: e.target.value}))}
            required
            style={select}
          >
            <option value="">Select</option>
            {items.map(i => <option key={i.id} value={i.name}>{i.name}</option>)}
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
          <label style={{ width: 140 }}>Quantity</label>
          <input
            type="number"
            min="1"
            value={form.quantity}
            onChange={(e)=>setForm(f=>({...f, quantity: e.target.value}))}
            required
            style={input}
          />
        </div>

        <div style={formRow}>
          <label style={{ width: 140 }}>Purpose</label>
          <input
            type="text"
            value={form.purpose}
            onChange={(e)=>setForm(f=>({...f, purpose: e.target.value}))}
            placeholder="Reason / department (optional)"
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
            <option>Approved</option>
            <option>Rejected</option>
          </select>
        </div>

        <button style={button} disabled={loading}>Submit</button>
      </form>
    </div>
  );
}

/* styles */
const container = { padding: "20px", maxWidth: "1000px", margin: "0 auto" };
const title = { fontSize: "24px", fontWeight: "bold", marginBottom: "10px" };
const filtersRow = { display: "flex", gap: "15px", margin: "15px 0", alignItems: "center" };
const formStyle = { marginTop: "20px", padding: "15px", border: "1px solid #ddd", borderRadius: "8px" };
const formRow = { display: "flex", alignItems: "center", gap: "10px", marginBottom: "10px" };
const input = { padding: "8px", border: "1px solid #ccc", borderRadius: "4px", width: "100%" };
const select = { padding: "8px", border: "1px solid #ccc", borderRadius: "4px" };
const button = { padding: "8px 12px", background: "#007bff", color: "#fff", border: "none", borderRadius: "4px", cursor: "pointer", marginRight: "8px" };
const dangerButton = { ...button, background: "#dc3545" };
const ghostButton = { ...button, background: "#6c757d" };
const successStyle = { color: "green", marginBottom: "10px" };
const errorStyle = { color: "crimson", marginBottom: "10px" };
const thStyle = { padding: "12px", borderBottom: "1px solid #ccc", textAlign: "left" };
const tdStyle = { padding: "10px", borderBottom: "1px solid #eee", verticalAlign: "top" };

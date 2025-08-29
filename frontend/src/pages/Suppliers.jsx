import { useEffect, useMemo, useState } from "react";
import { Suppliers } from "../lib/api";

export default function SuppliersPage() {
  const [data, setData] = useState([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [q, setQ] = useState("");

  const [name, setName] = useState("");
  const [email, setEmail] = useState("");
  const [phone, setPhone] = useState("");

  const [editId, setEditId] = useState(null);
  const [editName, setEditName] = useState("");
  const [editEmail, setEditEmail] = useState("");
  const [editPhone, setEditPhone] = useState("");

  const [statusMsg, setStatusMsg] = useState("");
  const [errorMsg, setErrorMsg] = useState("");

  const canCreate = useMemo(() => name.trim().length > 0, [name]);
  const canUpdate = useMemo(() => editName.trim().length > 0, [editName]);

  async function load() {
    setErrorMsg("");
    try {
      const res = await Suppliers.list({ q, page, pageSize });
      setData(res?.data ?? []);
      setTotal(res?.total ?? (res?.data?.length ?? 0));
    } catch (e) {
      setErrorMsg(e.message);
    }
  }

  useEffect(() => {
    load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [q, page, pageSize]);

  async function onCreate(e) {
    e.preventDefault();
    setStatusMsg("");
    setErrorMsg("");
    try {
      await Suppliers.create({
        name: name.trim(),
        contactEmail: email || null,
        phone: phone || null,
      });
      setName(""); setEmail(""); setPhone("");
      setPage(1);
      await load();
      setStatusMsg("Supplier created.");
    } catch (e) {
      setErrorMsg(e.message);
    }
  }

  function startEdit(s) {
    setEditId(s.id);
    setEditName(s.name);
    setEditEmail(s.contactEmail || "");
    setEditPhone(s.phone || "");
  }

  function cancelEdit() {
    setEditId(null);
    setEditName(""); setEditEmail(""); setEditPhone("");
  }

  async function onUpdate(e) {
    e.preventDefault();
    setStatusMsg("");
    setErrorMsg("");
    try {
      await Suppliers.update(editId, {
        name: editName.trim(),
        contactEmail: editEmail || null,
        phone: editPhone || null,
      });
      cancelEdit();
      await load();
      setStatusMsg("Supplier updated.");
    } catch (e) {
      setErrorMsg(e.message);
    }
  }

  async function onDelete(id) {
    if (!confirm("Delete this supplier?")) return;
    setStatusMsg("");
    setErrorMsg("");
    try {
      await Suppliers.remove(id);
      await load();
      setStatusMsg("Supplier deleted.");
    } catch (e) {
      setErrorMsg(e.message);
    }
  }

  const totalPages = Math.max(1, Math.ceil(total / pageSize));

  return (
    <div style={container}>
      <h1 style={title}>Suppliers</h1>

      {/* Messages */}
      {statusMsg && <div style={successStyle}>{statusMsg}</div>}
      {errorMsg && <div style={errorStyle}>{errorMsg}</div>}

      {/* Toolbar */}
      <div style={toolbar}>
        <div style={{ display: "flex", gap: "8px", alignItems: "center" }}>
          <label>Search:</label>
          <input
            value={q}
            onChange={(e) => { setQ(e.target.value); setPage(1); }}
            placeholder="name, email, phone"
            style={input}
          />
        </div>
        <div style={{ display: "flex", gap: "8px", alignItems: "center" }}>
          <label>Page size</label>
          <select value={pageSize} onChange={(e) => setPageSize(Number(e.target.value))} style={select}>
            {[5, 10, 20, 50].map(n => <option key={n} value={n}>{n}</option>)}
          </select>
        </div>
      </div>

      {/* Table */}
      <table style={{ width: "100%", borderCollapse: "collapse" }}>
        <thead>
          <tr style={{ background: "#f0f0f0" }}>
            <th style={thStyle}>Name</th>
            <th style={thStyle}>Email</th>
            <th style={thStyle}>Phone</th>
            <th style={thStyle}>Actions</th>
          </tr>
        </thead>
        <tbody>
          {data.map((s) => (
            <tr key={s.id}>
              <td style={tdStyle}>
                {editId === s.id ? (
                  <input value={editName} onChange={(e) => setEditName(e.target.value)} style={input} />
                ) : s.name}
              </td>
              <td style={tdStyle}>
                {editId === s.id ? (
                  <input value={editEmail} onChange={(e) => setEditEmail(e.target.value)} style={input} />
                ) : (s.contactEmail || "-")}
              </td>
              <td style={tdStyle}>
                {editId === s.id ? (
                  <input value={editPhone} onChange={(e) => setEditPhone(e.target.value)} style={input} />
                ) : (s.phone || "-")}
              </td>
              <td style={tdStyle}>
                {editId === s.id ? (
                  <>
                    <button style={button} onClick={onUpdate} disabled={!canUpdate}>Save</button>
                    <button style={secondaryButton} onClick={cancelEdit}>Cancel</button>
                  </>
                ) : (
                  <>
                    <button style={button} onClick={() => startEdit(s)}>Edit</button>
                    <button style={dangerButton} onClick={() => onDelete(s.id)}>Delete</button>
                  </>
                )}
              </td>
            </tr>
          ))}
          {data.length === 0 && (
            <tr><td style={tdStyle} colSpan={4}>No suppliers</td></tr>
          )}
        </tbody>
      </table>

      {/* Pager */}
      <div style={pagerRow}>
        <button style={button} disabled={page <= 1} onClick={() => setPage(p => p - 1)}>Prev</button>
        <span>Page {page} of {totalPages}</span>
        <button style={button} disabled={page >= totalPages} onClick={() => setPage(p => p + 1)}>Next</button>
      </div>

      {/* Create */}
      <form onSubmit={onCreate} style={formStyle}>
        <h2>Add supplier</h2>
        <div style={formRow}>
          <label style={{ width: 90 }}>Name:</label>
          <input value={name} onChange={(e) => setName(e.target.value)} required maxLength={150} style={input} />
        </div>
        <div style={formRow}>
          <label style={{ width: 90 }}>Email:</label>
          <input value={email} onChange={(e) => setEmail(e.target.value)} style={input} />
        </div>
        <div style={formRow}>
          <label style={{ width: 90 }}>Phone:</label>
          <input value={phone} onChange={(e) => setPhone(e.target.value)} style={input} />
        </div>
        <button style={button} disabled={!canCreate}>Create</button>
      </form>
    </div>
  );
}

/* ==== inline styles (to match your Inventory.jsx) ==== */
const container = { padding: "20px", maxWidth: "1000px", margin: "0 auto" };
const title = { fontSize: "24px", fontWeight: "bold", marginBottom: "10px" };
const toolbar = { display: "flex", justifyContent: "space-between", margin: "15px 0" };
const formStyle = { marginTop: "20px", padding: "15px", border: "1px solid #ddd", borderRadius: "8px" };
const formRow = { display: "flex", alignItems: "center", gap: "10px", marginBottom: "10px" };
const pagerRow = { display: "flex", alignItems: "center", gap: "10px", marginTop: "12px" };

const input = { padding: "8px", border: "1px solid #ccc", borderRadius: "4px", width: "100%" };
const select = { padding: "8px", border: "1px solid #ccc", borderRadius: "4px" };

const button = { padding: "8px 12px", background: "#007bff", color: "#fff", border: "none", borderRadius: "4px", cursor: "pointer", marginRight: "8px" };
const secondaryButton = { ...button, background: "#6c757d" };
const dangerButton = { ...button, background: "#dc3545" };

const successStyle = { color: "green", marginBottom: "10px" };
const errorStyle = { color: "crimson", marginBottom: "10px" };
const thStyle = { padding: "12px", borderBottom: "1px solid #ccc", textAlign: "left" };
const tdStyle = { padding: "10px", borderBottom: "1px solid #eee" };

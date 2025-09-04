import React, { useEffect, useMemo, useState } from "react";
import { Items, Categories, Offices, StockLevels, Suppliers } from "../lib/api";
import RoleGate from "../auth/RoleGate";
import { useAuth } from "../auth/AuthContext";
import { can } from "../auth/permissions";

export default function Inventory() {
  const { roles = [] } = useAuth();
  const showActionsCol = can(roles, "InventoryManage"); // show actions col if you can manage

  // data
  const [items, setItems] = useState([]);
  const [categories, setCategories] = useState([]);
  const [offices, setOffices] = useState([]);
  const [suppliers, setSuppliers] = useState([]);
  const [stockLevels, setStockLevels] = useState([]);

  // ui state
  const [categoryFilter, setCategoryFilter] = useState("All");
  const [officeFilter, setOfficeFilter] = useState("All");
  const [statusMsg, setStatusMsg] = useState("");
  const [errorMsg, setErrorMsg] = useState("");

  // create form
  const [newItem, setNewItem] = useState({ name: "", category: "Core", stock: "", office: "London" });

  // edit stock row state
  const [editId, setEditId] = useState(null);
  const [editQty, setEditQty] = useState(0);
  const [editThresh, setEditThresh] = useState("");

  // edit item modal state
  const [showItemModal, setShowItemModal] = useState(false);
  const [editItemId, setEditItemId] = useState(null);
  const [editItemName, setEditItemName] = useState("");
  const [editItemCategoryId, setEditItemCategoryId] = useState("");
  const [editItemSupplierId, setEditItemSupplierId] = useState("");

  // rows for grid
  const rows = useMemo(() => {
    const itemById = new Map(items.map(i => [i.id, i]));
    const catById = new Map(categories.map(c => [c.id, c.name]));
    const officeById = new Map(offices.map(o => [o.id, o.name]));

    return stockLevels.map(sl => {
      const it = itemById.get(sl.itemId);
      const itemName = sl.itemName ?? it?.name ?? `Item ${sl.itemId}`;
      const catName = it?.categoryName ?? (it?.categoryId ? catById.get(it.categoryId) : null) ?? "Unknown";
      const officeName = sl.officeName ?? officeById.get(sl.officeId) ?? `Office ${sl.officeId}`;
      return {
        id: sl.id,
        itemId: sl.itemId,
        officeId: sl.officeId,
        key: `${sl.itemId}-${sl.officeId}`,
        name: itemName,
        category: catName,
        office: officeName,
        stock: sl.quantity ?? 0,
        reorderThreshold: sl.reorderThreshold ?? null,
      };
    })
    .filter(r => (categoryFilter === "All" || r.category === categoryFilter)
              && (officeFilter === "All" || r.office === officeFilter))
    .sort((a, b) => a.name.localeCompare(b.name));
  }, [items, categories, offices, stockLevels, categoryFilter, officeFilter]);

  // load data
  async function loadAll() {
    const [catRes, offRes, itemRes, slRes, supRes] = await Promise.all([
      Categories.list(),
      Offices.list(),
      Items.list({ page: 1, pageSize: 500 }),
      StockLevels.list(),
      Suppliers.list({ page: 1, pageSize: 500 }),
    ]);
    setCategories(Array.isArray(catRes) ? catRes : catRes?.data ?? []);
    setOffices(Array.isArray(offRes) ? offRes : offRes?.data ?? []);
    setItems(itemRes?.data ?? itemRes ?? []);
    setStockLevels(Array.isArray(slRes) ? slRes : slRes?.data ?? []);
    setSuppliers(supRes?.data ?? []);
  }

  useEffect(() => {
    (async () => {
      try { await loadAll(); }
      catch (e) { console.error(e); setErrorMsg(e.message); }
    })();
  }, []);

  // create
  async function handleCreate(e) {
    e.preventDefault();
    setStatusMsg(""); setErrorMsg("");
    try {
      const cat = categories.find(c => c.name === newItem.category);
      const off = offices.find(o => o.name === newItem.office);
      if (!cat) throw new Error(`Category '${newItem.category}' not found`);
      if (!off) throw new Error(`Office '${newItem.office}' not found`);

      const createdItem = await Items.create({
        name: newItem.name.trim(),
        description: newItem.name.trim(),
        categoryId: cat.id,
        supplierId: null
      });
      const itemId = createdItem?.id ?? createdItem?.data?.id;
      if (!itemId) throw new Error("Could not determine new Item ID.");

      const qty = Number(newItem.stock);
      if (Number.isNaN(qty)) throw new Error("Stock must be a number");

      await StockLevels.create({ itemId, officeId: off.id, quantity: qty, reorderThreshold: 10 });

      await loadAll();
      setNewItem({ name: "", category: "Core", stock: "", office: "London" });
      setStatusMsg("Item added successfully.");
    } catch (e) {
      setErrorMsg(e.message);
    }
  }

  // stock editing
  function startEdit(row) {
    setEditId(row.id);
    setEditQty(row.stock);
    setEditThresh(row.reorderThreshold ?? "");
  }
  function cancelEdit() { setEditId(null); setEditQty(0); setEditThresh(""); }

  async function saveEdit() {
    try {
      const payload = {
        quantity: Number(editQty),
        reorderThreshold: (editThresh === "" || editThresh == null) ? 0 : Number(editThresh),
      };
      if (Number.isNaN(payload.quantity) || Number.isNaN(payload.reorderThreshold))
        throw new Error("Please enter valid numbers.");
      await StockLevels.update(editId, payload);
      cancelEdit();
      const slRes = await StockLevels.list();
      setStockLevels(Array.isArray(slRes) ? slRes : slRes?.data ?? []);
      setStatusMsg("Stock level updated.");
    } catch (e) {
      setErrorMsg(e.message);
    }
  }

  async function deleteRow(id) {
    if (!confirm("Delete this stock row for this office?")) return;
    try {
      await StockLevels.remove(id);
      const slRes = await StockLevels.list();
      setStockLevels(Array.isArray(slRes) ? slRes : slRes?.data ?? []);
      setStatusMsg("Stock level deleted.");
    } catch (e) {
      setErrorMsg(e.message);
    }
  }

  // quick +/- adjust
  async function adjustRow(id, delta) {
    try {
      await StockLevels.adjust(id, delta);
      const slRes = await StockLevels.list();
      setStockLevels(Array.isArray(slRes) ? slRes : slRes?.data ?? []);
    } catch (e) {
      setErrorMsg(e.message);
    }
  }

  // item modal
  function openItemModal(row) {
    const it = items.find(x => x.id === row.itemId);
    if (!it) { setErrorMsg("Item not found"); return; }
    setEditItemId(it.id);
    setEditItemName(it.name);
    setEditItemCategoryId(it.categoryId ?? "");
    setEditItemSupplierId(it.supplierId ?? "");
    setShowItemModal(true);
  }
  function closeItemModal() { setShowItemModal(false); setEditItemId(null); }

  async function saveItemModal() {
    try {
      const payload = {
        name: editItemName.trim(),
        description: editItemName.trim(),
        categoryId: Number(editItemCategoryId),
        supplierId: editItemSupplierId ? Number(editItemSupplierId) : null
      };
      await Items.update(editItemId, payload);
      await loadAll();
      setStatusMsg("Item updated.");
      closeItemModal();
    } catch (e) {
      setErrorMsg(e.message);
    }
  }

  async function deleteItemWithRows(itemId) {
    if (!confirm("Delete this item and all its stock rows?")) return;
    try {
      const rows = await StockLevels.list({ itemId });
      const list = Array.isArray(rows) ? rows : rows?.data ?? [];
      for (const r of list) {
        await StockLevels.remove(r.id);
      }
      await Items.remove(itemId);
      await loadAll();
      setStatusMsg("Item and related stock rows deleted.");
      closeItemModal();
    } catch (e) {
      setErrorMsg(e.message);
    }
  }

  const baseHeader = (
    <>
      <th style={thStyle}>Item</th>
      <th style={thStyle}>Category</th>
      <th style={thStyle}>Office</th>
      <th style={thStyle}>Stock</th>
      <th style={thStyle}>Reorder Threshold</th>
    </>
  );
  const columnsCount = showActionsCol ? 6 : 5;

  return (
    <div style={container}>
      <h1 style={title}>Inventory</h1>

      <div style={filtersRow}>
        <div>
          <label>Filter by Category: </label>
          <select value={categoryFilter} onChange={(e) => setCategoryFilter(e.target.value)}>
            <option>All</option>
            {categories.map(c => <option key={c.id}>{c.name}</option>)}
          </select>
        </div>
        <div>
          <label>Filter by Office: </label>
          <select value={officeFilter} onChange={(e) => setOfficeFilter(e.target.value)}>
            <option>All</option>
            {offices.map(o => <option key={o.id}>{o.name}</option>)}
          </select>
        </div>
      </div>

      {statusMsg && <div style={successStyle}>{statusMsg}</div>}
      {errorMsg && <div style={errorStyle}>{errorMsg}</div>}

      <table style={{ width: "100%", borderCollapse: "collapse" }}>
        <thead>
          <tr style={{ background: "#f0f0f0" }}>
            {baseHeader}
            {showActionsCol && <th style={thStyle}>Actions</th>}
          </tr>
        </thead>
        <tbody>
          {rows.map((row) => {
            const isLow = row.reorderThreshold != null && row.stock <= row.reorderThreshold;
            const isEditing = editId === row.id;
            return (
              <tr key={row.key} style={isLow ? { background: "#fff3cd" } : undefined}>
                <td style={tdStyle}>{row.name}</td>
                <td style={tdStyle}>{row.category}</td>
                <td style={tdStyle}>{row.office}</td>
                <td style={tdStyle}>
                  {isEditing ? (
                    <input type="number" value={editQty} onChange={(e) => setEditQty(e.target.value)} style={inputInline} />
                  ) : (
                    <>
                      <RoleGate feature="InventoryManage">
                        <button style={tinyBtn} onClick={() => adjustRow(row.id, -1)}>-</button>
                      </RoleGate>
                      <span style={{ margin: "0 8px" }}>{row.stock}</span>
                      <RoleGate feature="InventoryManage">
                        <button style={tinyBtn} onClick={() => adjustRow(row.id, +1)}>+</button>
                      </RoleGate>
                    </>
                  )}
                </td>
                <td style={tdStyle}>
                  {isEditing ? (
                    <input type="number" value={editThresh} onChange={(e) => setEditThresh(e.target.value)} placeholder="0" style={inputInline} />
                  ) : (
                    row.reorderThreshold != null ? row.reorderThreshold : "-"
                  )}
                </td>

                {showActionsCol && (
                  <td style={tdStyle}>
                    {isEditing ? (
                      <>
                        <button style={button} onClick={saveEdit}>Save</button>
                        <button style={secondaryButton} onClick={cancelEdit}>Cancel</button>
                      </>
                    ) : (
                      <>
                        <button style={button} onClick={() => startEdit(row)}>Edit</button>
                        <button style={secondaryButton} onClick={() => openItemModal(row)}>Edit Item</button>

                        {/* Delete = SuperAdmin only */}
                        <RoleGate feature="InventoryDelete">
                          <button style={dangerButton} onClick={() => deleteRow(row.id)}>Delete</button>
                        </RoleGate>
                      </>
                    )}
                  </td>
                )}
              </tr>
            );
          })}
          {rows.length === 0 && (
            <tr><td style={tdStyle} colSpan={columnsCount}>No items</td></tr>
          )}
        </tbody>
      </table>

      {/* Add new item form — Admin/SuperAdmin */}
      <RoleGate feature="InventoryManage">
        <form onSubmit={handleCreate} style={formStyle}>
          <h2>Add New Item</h2>
          <div style={formRow}>
            <label style={{ width: 90 }}>Name:</label>
            <input value={newItem.name} onChange={(e) => setNewItem({ ...newItem, name: e.target.value })} required maxLength={80} style={input} />
          </div>
          <div style={formRow}>
            <label style={{ width: 90 }}>Category:</label>
            <select value={newItem.category} onChange={(e) => setNewItem({ ...newItem, category: e.target.value })} style={select}>
              {categories.map(c => <option key={c.id}>{c.name}</option>)}
            </select>
          </div>
          <div style={formRow}>
            <label style={{ width: 90 }}>Office:</label>
            <select value={newItem.office} onChange={(e) => setNewItem({ ...newItem, office: e.target.value })} style={select}>
              {offices.map(o => <option key={o.id}>{o.name}</option>)}
            </select>
          </div>
          <div style={formRow}>
            <label style={{ width: 90 }}>Stock:</label>
            <input type="number" value={newItem.stock} onChange={(e) => setNewItem({ ...newItem, stock: e.target.value })} required style={input} />
          </div>
          <button style={button} type="submit">Add Item</button>
        </form>
      </RoleGate>

      {/* Item Modal — Admin/SuperAdmin */}
      {showItemModal && (
        <RoleGate feature="InventoryManage">
          <div style={modalBackdrop} onClick={closeItemModal}>
            <div style={modalBody} onClick={(e) => e.stopPropagation()}>
              <h3>Edit Item</h3>
              <div style={formRow}>
                <label style={{ width: 120 }}>Name:</label>
                <input value={editItemName} onChange={(e) => setEditItemName(e.target.value)} style={input} />
              </div>
              <div style={formRow}>
                <label style={{ width: 120 }}>Category:</label>
                <select value={editItemCategoryId} onChange={(e) => setEditItemCategoryId(e.target.value)} style={select}>
                  {categories.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
                </select>
              </div>
              <div style={formRow}>
                <label style={{ width: 120 }}>Supplier:</label>
                <select value={editItemSupplierId ?? ""} onChange={(e) => setEditItemSupplierId(e.target.value)} style={select}>
                  <option value="">None</option>
                  {suppliers.map(s => <option key={s.id} value={s.id}>{s.name}</option>)}
                </select>
              </div>
              <div style={{ display: "flex", gap: 8, marginTop: 12 }}>
                <button style={button} onClick={saveItemModal}>Save Item</button>
                <button style={secondaryButton} onClick={closeItemModal}>Close</button>

                {/* Delete Item = SuperAdmin only */}
                <RoleGate feature="InventoryDelete">
                  <button style={dangerButton} onClick={() => deleteItemWithRows(editItemId)}>Delete Item</button>
                </RoleGate>
              </div>
            </div>
          </div>
        </RoleGate>
      )}
    </div>
  );
}

/* styles (unchanged) */
const container = { padding: "20px", maxWidth: "1000px", margin: "0 auto" };
const title = { fontSize: "24px", fontWeight: "bold", marginBottom: "10px" };
const filtersRow = { display: "flex", gap: "15px", margin: "15px 0" };
const formStyle = { marginTop: "20px", padding: "15px", border: "1px solid #ddd", borderRadius: "8px" };
const formRow = { display: "flex", alignItems: "center", gap: "10px", marginBottom: "10px" };
const input = { padding: "8px", border: "1px solid #ccc", borderRadius: "4px", width: "100%" };
const inputInline = { padding: "6px", border: "1px solid #ccc", borderRadius: "4px", width: 90 };
const select = { padding: "8px", border: "1px solid #ccc", borderRadius: "4px" };
const button = { padding: "8px 12px", background: "#007bff", color: "#fff", border: "none", borderRadius: "4px", cursor: "pointer", marginRight: "8px" };
const secondaryButton = { ...button, background: "#6c757d" };
const dangerButton = { ...button, background: "#dc3545" };
const tinyBtn = { padding: "2px 8px", border: "1px solid #ccc", background: "#f8f9fa", borderRadius: "4px", cursor: "pointer" };
const successStyle = { color: "green", marginBottom: "10px" };
const errorStyle = { color: "crimson", marginBottom: "10px" };
const thStyle = { padding: "12px", borderBottom: "1px solid #ccc", textAlign: "left" };
const tdStyle = { padding: "10px", borderBottom: "1px solid #eee" };
const modalBackdrop = { position: "fixed", inset: 0, background: "rgba(0,0,0,.3)", display: "flex", alignItems: "center", justifyContent: "center", padding: 16 };
const modalBody = { background: "#fff", borderRadius: 8, padding: 16, width: 480, maxWidth: "90%" };

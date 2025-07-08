import React, { useState, useEffect } from "react";
import inventoryItems from "../data/inventoryData";

export default function Inventory() {
  const [items, setItems] = useState([]);
  const [filteredItems, setFilteredItems] = useState([]);
  const [categoryFilter, setCategoryFilter] = useState("All");
  const [officeFilter, setOfficeFilter] = useState("All");

  // Form state
  const [newItem, setNewItem] = useState({
    name: "",
    category: "Core",
    stock: "",
    office: "London",
  });

  // Load mock data on mount
  useEffect(() => {
    setItems(inventoryItems);
    setFilteredItems(inventoryItems);
  }, []);

  // Update filtered list
  useEffect(() => {
    let filtered = [...items];
    if (categoryFilter !== "All") {
      filtered = filtered.filter((item) => item.category === categoryFilter);
    }
    if (officeFilter !== "All") {
      filtered = filtered.filter((item) => item.office === officeFilter);
    }
    setFilteredItems(filtered);
  }, [categoryFilter, officeFilter, items]);

  // Handle form input change
  const handleChange = (e) => {
    setNewItem({ ...newItem, [e.target.name]: e.target.value });
  };

  // Handle form submit
  const handleAddItem = (e) => {
    e.preventDefault();

    const itemToAdd = {
      id: Date.now(),
      name: newItem.name,
      category: newItem.category,
      stock: parseInt(newItem.stock, 10),
      office: newItem.office,
    };

    const updatedItems = [...items, itemToAdd];
    setItems(updatedItems);
    setNewItem({ name: "", category: "Core", stock: "", office: "London" });
  };

  return (
    <div style={{ padding: "2rem" }}>
      <h2>Inventory</h2>

      {/* === Add New Item Form === */}
      <form onSubmit={handleAddItem} style={{ marginBottom: "2rem", border: "1px solid #ccc", padding: "1rem" }}>
        <h3>Add New Item</h3>
        <input
          type="text"
          name="name"
          placeholder="Item name"
          value={newItem.name}
          onChange={handleChange}
          required
          style={inputStyle}
        />
        <select name="category" value={newItem.category} onChange={handleChange} style={inputStyle}>
          <option value="Core">Core</option>
          <option value="Special">Special</option>
          <option value="Printed">Printed</option>
        </select>
        <input
          type="number"
          name="stock"
          placeholder="Stock quantity"
          value={newItem.stock}
          onChange={handleChange}
          required
          style={inputStyle}
        />
        <select name="office" value={newItem.office} onChange={handleChange} style={inputStyle}>
          <option value="London">London</option>
          <option value="Manchester">Manchester</option>
          <option value="Birmingham">Birmingham</option>
        </select>
        <button type="submit" style={{ padding: "10px 16px", marginTop: "10px" }}>Add Item</button>
      </form>

      {/* === Filters === */}
      <div style={{ display: "flex", gap: "1rem", marginBottom: "1rem" }}>
        <div>
          <label>Filter by Category: </label>
          <select value={categoryFilter} onChange={(e) => setCategoryFilter(e.target.value)}>
            <option>All</option>
            <option>Core</option>
            <option>Special</option>
            <option>Printed</option>
          </select>
        </div>

        <div>
          <label>Filter by Office: </label>
          <select value={officeFilter} onChange={(e) => setOfficeFilter(e.target.value)}>
            <option>All</option>
            <option>London</option>
            <option>Manchester</option>
            <option>Birmingham</option>
          </select>
        </div>
      </div>

      {/* === Inventory Table === */}
      <table style={{ width: "100%", borderCollapse: "collapse" }}>
        <thead>
          <tr style={{ background: "#f0f0f0" }}>
            <th style={thStyle}>Item</th>
            <th style={thStyle}>Category</th>
            <th style={thStyle}>Stock</th>
            <th style={thStyle}>Office</th>
          </tr>
        </thead>
        <tbody>
          {filteredItems.map((item) => (
            <tr key={item.id}>
              <td style={tdStyle}>{item.name}</td>
              <td style={tdStyle}>{item.category}</td>
              <td style={tdStyle}>{item.stock}</td>
              <td style={tdStyle}>{item.office}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

// === Reusable Styles ===
const thStyle = {
  padding: "12px",
  borderBottom: "1px solid #ccc",
  textAlign: "left",
};

const tdStyle = {
  padding: "10px",
  borderBottom: "1px solid #eee",
};

const inputStyle = {
  margin: "0.5rem 0",
  padding: "8px",
  width: "100%",
  maxWidth: "300px",
  display: "block",
};


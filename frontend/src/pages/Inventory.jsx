import React, { useState, useEffect } from "react";
import inventoryItems from "../data/inventoryData";

export default function Inventory() {
  const [items, setItems] = useState([]);
  const [filteredItems, setFilteredItems] = useState([]);
  const [categoryFilter, setCategoryFilter] = useState("All");
  const [officeFilter, setOfficeFilter] = useState("All");

  // Load items from mock data
  useEffect(() => {
    setItems(inventoryItems);
    setFilteredItems(inventoryItems);
  }, []);

  // Filter logic
  useEffect(() => {
    let filtered = items;

    if (categoryFilter !== "All") {
      filtered = filtered.filter((item) => item.category === categoryFilter);
    }

    if (officeFilter !== "All") {
      filtered = filtered.filter((item) => item.office === officeFilter);
    }

    setFilteredItems(filtered);
  }, [categoryFilter, officeFilter, items]);

  return (
    <div style={{ padding: "2rem" }}>
      <h2>Inventory</h2>

      {/* Filters */}
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

      {/* Table */}
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

const thStyle = {
  padding: "12px",
  borderBottom: "1px solid #ccc",
  textAlign: "left",
};

const tdStyle = {
  padding: "10px",
  borderBottom: "1px solid #eee",
};

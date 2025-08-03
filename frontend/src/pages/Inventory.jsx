import React, { useState, useEffect } from "react";
import inventoryItems from "../data/inventoryData";

export default function Inventory() {
  const userRole = "admin"; // "employee", "manager", "admin"

  const [items, setItems] = useState([]);
  const [filteredItems, setFilteredItems] = useState([]);
  const [categoryFilter, setCategoryFilter] = useState("All");
  const [officeFilter, setOfficeFilter] = useState("All");

  const [newItem, setNewItem] = useState({
    name: "",
    category: "Core",
    stock: "",
    office: "London",
  });

  const [successMsg, setSuccessMsg] = useState("");

  useEffect(() => {
    setItems(inventoryItems);
    setFilteredItems(inventoryItems);
  }, []);

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

  const handleChange = (e) => {
    setNewItem({ ...newItem, [e.target.name]: e.target.value });
  };

  const handleAddItem = (e) => {
    e.preventDefault();

    if (!newItem.name.trim() || !newItem.stock || newItem.stock <= 0) {
      alert("Please enter a valid item name and positive stock quantity.");
      return;
    }

    const itemToAdd = {
      id: Date.now(),
      name: newItem.name.trim(),
      category: newItem.category,
      stock: parseInt(newItem.stock, 10),
      office: newItem.office,
    };

    setItems([...items, itemToAdd]);
    setNewItem({ name: "", category: "Core", stock: "", office: "London" });
    setSuccessMsg("Item added successfully!");

    setTimeout(() => setSuccessMsg(""), 3000);
  };

  const handleDelete = (id) => {
    setItems(items.filter((item) => item.id !== id));
  };

  return (
    <div style={containerStyle}>
      <h2>Inventory</h2>

      {/* Add New Item Form */}
      {userRole === "admin" && (
        <form onSubmit={handleAddItem} style={formStyle}>
          <h3>Add New Item</h3>
          {successMsg && <p style={successStyle}>{successMsg}</p>}

          <input
            type="text"
            name="name"
            placeholder="Item name"
            value={newItem.name}
            onChange={handleChange}
            style={inputStyle}
          />
          <select
            name="category"
            value={newItem.category}
            onChange={handleChange}
            style={inputStyle}
          >
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
            style={inputStyle}
          />
          <select
            name="office"
            value={newItem.office}
            onChange={handleChange}
            style={inputStyle}
          >
            <option value="London">London</option>
            <option value="Manchester">Manchester</option>
            <option value="Birmingham">Birmingham</option>
          </select>
          <button type="submit" style={buttonStyle}>Add Item</button>
        </form>
      )}

      {/* Filters */}
      <div style={{ display: "flex", gap: "1rem", marginBottom: "1rem" }}>
        <div>
          <label>Filter by Category: </label>
          <select
            value={categoryFilter}
            onChange={(e) => setCategoryFilter(e.target.value)}
          >
            <option>All</option>
            <option>Core</option>
            <option>Special</option>
            <option>Printed</option>
          </select>
        </div>
        <div>
          <label>Filter by Office: </label>
          <select
            value={officeFilter}
            onChange={(e) => setOfficeFilter(e.target.value)}
          >
            <option>All</option>
            <option>London</option>
            <option>Manchester</option>
            <option>Birmingham</option>
          </select>
        </div>
      </div>

      {/* Inventory Table */}
      <table style={{ width: "100%", borderCollapse: "collapse" }}>
        <thead>
          <tr style={{ background: "#f0f0f0" }}>
            <th style={thStyle}>Item</th>
            <th style={thStyle}>Category</th>
            <th style={thStyle}>Stock</th>
            <th style={thStyle}>Office</th>
            {userRole === "admin" && <th style={thStyle}>Actions</th>}
          </tr>
        </thead>
        <tbody>
          {filteredItems.map((item) => (
            <tr key={item.id}>
              <td style={tdStyle}>{item.name}</td>
              <td style={tdStyle}>{item.category}</td>
              <td style={tdStyle}>{item.stock}</td>
              <td style={tdStyle}>{item.office}</td>
              {userRole === "admin" && (
                <td style={tdStyle}>
                  <button
                    onClick={() => handleDelete(item.id)}
                    style={deleteButton}
                  >
                    Delete
                  </button>
                </td>
              )}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

// === Styles ===
const containerStyle = {
  background: "#fff",
  borderRadius: "10px",
  padding: "20px",
  boxShadow: "0 0 8px rgba(0,0,0,0.1)",
};

const formStyle = {
  marginBottom: "2rem",
  padding: "1rem",
  border: "1px solid #ccc",
  borderRadius: "8px",
  maxWidth: "400px",
};

const inputStyle = {
  display: "block",
  margin: "10px 0",
  padding: "8px",
  width: "100%",
};

const buttonStyle = {
  padding: "8px 16px",
  backgroundColor: "#007bff",
  color: "#fff",
  border: "none",
  borderRadius: "4px",
  cursor: "pointer",
};

const deleteButton = {
  ...buttonStyle,
  backgroundColor: "red",
};

const successStyle = {
  color: "green",
  marginBottom: "10px",
};

const thStyle = {
  padding: "12px",
  borderBottom: "1px solid #ccc",
  textAlign: "left",
};

const tdStyle = {
  padding: "10px",
  borderBottom: "1px solid #eee",
};

import React from "react";

const mockInventory = [
  {
    id: 1,
    name: "A4 Paper",
    category: "Core",
    stock: 120,
    office: "London",
  },
  {
    id: 2,
    name: "Branded Envelopes",
    category: "Printed",
    stock: 45,
    office: "Manchester",
  },
  {
    id: 3,
    name: "Custom Notebooks",
    category: "Special",
    stock: 15,
    office: "Birmingham",
  },
];

export default function Inventory() {
  return (
    <div style={{ padding: "2rem" }}>
      <h2>Inventory</h2>
      <table style={{ width: "100%", borderCollapse: "collapse", marginTop: "1rem" }}>
        <thead>
          <tr style={{ background: "#f0f0f0" }}>
            <th style={thStyle}>Item</th>
            <th style={thStyle}>Category</th>
            <th style={thStyle}>Stock</th>
            <th style={thStyle}>Office</th>
          </tr>
        </thead>
        <tbody>
          {mockInventory.map((item) => (
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
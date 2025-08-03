// frontend/src/pages/Inventory.jsx
import React, { useState } from "react";
import {
  PieChart,
  Pie,
  Cell,
  Tooltip,
  Legend,
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  ResponsiveContainer
} from "recharts";

const Inventory = () => {
  const [searchTerm, setSearchTerm] = useState("");

  // Sample data
  const inventoryData = [
    { name: "Pens", quantity: 120 },
    { name: "Notebooks", quantity: 85 },
    { name: "Envelopes", quantity: 60 },
    { name: "Staplers", quantity: 25 },
    { name: "Markers", quantity: 40 }
  ];

  const officeRequests = [
    { office: "London", requests: 30 },
    { office: "Birmingham", requests: 20 },
    { office: "Leeds", requests: 15 },
    { office: "Manchester", requests: 18 }
  ];

  const COLORS = ["#0088FE", "#00C49F", "#FFBB28", "#FF8042", "#FF4560"];

  const filteredInventory = inventoryData.filter((item) =>
    item.name.toLowerCase().includes(searchTerm.toLowerCase())
  );

  return (
    <div style={{ padding: "1rem" }}>
      <h2>Inventory Overview</h2>

      {/* Search */}
      <input
        type="text"
        placeholder="Search inventory..."
        value={searchTerm}
        onChange={(e) => setSearchTerm(e.target.value)}
        style={{
          padding: "0.5rem",
          marginBottom: "1rem",
          width: "100%",
          maxWidth: "300px"
        }}
      />

      {/* Inventory Table */}
      <table
        style={{
          width: "100%",
          borderCollapse: "collapse",
          marginBottom: "2rem"
        }}
      >
        <thead>
          <tr style={{ background: "#f4f4f4" }}>
            <th style={{ padding: "0.5rem", border: "1px solid #ddd" }}>Item</th>
            <th style={{ padding: "0.5rem", border: "1px solid #ddd" }}>Quantity</th>
          </tr>
        </thead>
        <tbody>
          {filteredInventory.map((item, index) => (
            <tr key={index}>
              <td style={{ padding: "0.5rem", border: "1px solid #ddd" }}>
                {item.name}
              </td>
              <td style={{ padding: "0.5rem", border: "1px solid #ddd" }}>
                {item.quantity}
              </td>
            </tr>
          ))}
        </tbody>
      </table>

      {/* Charts */}
      <div
        style={{
          display: "flex",
          flexWrap: "wrap",
          gap: "2rem",
          justifyContent: "center"
        }}
      >
        {/* Pie Chart */}
        <div
          style={{
            flex: "1 1 350px",
            maxWidth: "500px",
            background: "#fff",
            padding: "1rem",
            borderRadius: "10px",
            boxShadow: "0 2px 8px rgba(0,0,0,0.1)"
          }}
        >
          <h3 style={{ textAlign: "center", marginBottom: "1rem" }}>
            Inventory Distribution
          </h3>
          <ResponsiveContainer width="100%" height={350}>
            <PieChart>
              <Pie
                data={inventoryData}
                dataKey="quantity"
                nameKey="name"
                cx="50%"
                cy="50%"
                outerRadius={120}
                label={({ name, percent }) =>
                  `${name} ${(percent * 100).toFixed(0)}%`
                }
              >
                {inventoryData.map((_, index) => (
                  <Cell key={index} fill={COLORS[index % COLORS.length]} />
                ))}
              </Pie>
              <Tooltip />
              <Legend verticalAlign="bottom" height={36} />
            </PieChart>
          </ResponsiveContainer>
        </div>

        {/* Bar Chart */}
        <div
          style={{
            flex: "1 1 500px",
            maxWidth: "700px",
            background: "#fff",
            padding: "1rem",
            borderRadius: "10px",
            boxShadow: "0 2px 8px rgba(0,0,0,0.1)"
          }}
        >
          <h3 style={{ textAlign: "center", marginBottom: "1rem" }}>
            Requests by Office
          </h3>
          <ResponsiveContainer width="100%" height={350}>
            <BarChart
              data={officeRequests}
              margin={{ top: 20, right: 40, left: 20, bottom: 40 }}
            >
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis
                dataKey="office"
                interval={0}
                angle={-15}
                textAnchor="end"
              />
              <YAxis />
              <Tooltip />
              <Bar dataKey="requests" fill="#8884d8" />
            </BarChart>
          </ResponsiveContainer>
        </div>
      </div>
    </div>
  );
};

export default Inventory;

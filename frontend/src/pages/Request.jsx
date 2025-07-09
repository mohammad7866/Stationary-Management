import React, { useState } from "react";

export default function Requests() {
  const userRole = "manager"; // Change to "employee", "manager", or "admin"

  const [requests, setRequests] = useState([
    {
      id: 1,
      item: "Pens",
      quantity: 10,
      purpose: "Team meeting",
      office: "London",
      status: "Pending",
    },
    {
      id: 2,
      item: "A4 Paper",
      quantity: 5,
      purpose: "Client reports",
      office: "Manchester",
      status: "Approved",
    },
  ]);

  const [form, setForm] = useState({
    item: "",
    quantity: "",
    purpose: "",
    office: "London", // default
  });

  const [successMsg, setSuccessMsg] = useState("");

  const handleChange = (e) => {
    setForm({ ...form, [e.target.name]: e.target.value });
  };

  const handleSubmit = (e) => {
    e.preventDefault();

    if (!form.item || !form.quantity || form.quantity <= 0 || !form.purpose) {
      alert("Please fill in all fields correctly.");
      return;
    }

    const newRequest = {
      id: Date.now(),
      ...form,
      quantity: parseInt(form.quantity, 10),
      status: "Pending",
    };

    setRequests([...requests, newRequest]);
    setForm({ item: "", quantity: "", purpose: "", office: "London" });
    setSuccessMsg("Request submitted!");

    setTimeout(() => setSuccessMsg(""), 3000);
  };

  const handleStatusChange = (id, status) => {
    const updated = requests.map((r) =>
      r.id === id ? { ...r, status } : r
    );
    setRequests(updated);
  };

  return (
    <div style={{ padding: "2rem" }}>
      <h2>Stationery Requests</h2>

      {userRole === "employee" && (
        <form onSubmit={handleSubmit} style={formStyle}>
          <h3>Submit Request</h3>
          {successMsg && <p style={{ color: "green" }}>{successMsg}</p>}
          <input
            type="text"
            name="item"
            placeholder="Item name"
            value={form.item}
            onChange={handleChange}
            style={inputStyle}
          />
          <input
            type="number"
            name="quantity"
            placeholder="Quantity"
            value={form.quantity}
            onChange={handleChange}
            style={inputStyle}
          />
          <input
            type="text"
            name="purpose"
            placeholder="Purpose"
            value={form.purpose}
            onChange={handleChange}
            style={inputStyle}
          />
          <select
            name="office"
            value={form.office}
            onChange={handleChange}
            style={inputStyle}
          >
            <option value="London">London</option>
            <option value="Manchester">Manchester</option>
            <option value="Birmingham">Birmingham</option>
          </select>
          <button type="submit" style={buttonStyle}>Submit</button>
        </form>
      )}

      <h3>All Requests</h3>
      <table style={{ width: "100%", borderCollapse: "collapse" }}>
        <thead>
          <tr style={{ background: "#f0f0f0" }}>
            <th style={thStyle}>Item</th>
            <th style={thStyle}>Quantity</th>
            <th style={thStyle}>Purpose</th>
            <th style={thStyle}>Office</th>
            <th style={thStyle}>Status</th>
            {userRole !== "employee" && <th style={thStyle}>Actions</th>}
          </tr>
        </thead>
        <tbody>
          {requests.map((req) => (
            <tr key={req.id}>
              <td style={tdStyle}>{req.item}</td>
              <td style={tdStyle}>{req.quantity}</td>
              <td style={tdStyle}>{req.purpose}</td>
              <td style={tdStyle}>{req.office}</td>
              <td style={tdStyle}>{req.status}</td>
              {userRole !== "employee" && (
                <td style={tdStyle}>
                  {req.status === "Pending" ? (
                    <>
                      <button
                        onClick={() => handleStatusChange(req.id, "Approved")}
                        style={approveButton}
                      >
                        Approve
                      </button>
                      <button
                        onClick={() => handleStatusChange(req.id, "Rejected")}
                        style={rejectButton}
                      >
                        Reject
                      </button>
                    </>
                  ) : (
                    <span style={{ color: req.status === "Approved" ? "green" : "red" }}>
                      {req.status}
                    </span>
                  )}
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

const approveButton = {
  ...buttonStyle,
  backgroundColor: "green",
  marginRight: "5px",
};

const rejectButton = {
  ...buttonStyle,
  backgroundColor: "red",
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

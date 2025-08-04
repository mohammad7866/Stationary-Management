// frontend/src/pages/Deliveries.jsx
import { useState } from "react";

export default function Deliveries() {
  const [deliveries, setDeliveries] = useState([
    { id: 1, product: "Printer Paper A4", supplier: "Office Depot", scheduledDate: "2025-08-05", arrivalDate: "2025-08-04", office: "London", status: "On Time" },
    { id: 2, product: "Pens - Black", supplier: "Staples", scheduledDate: "2025-08-06", arrivalDate: "", office: "Manchester", status: "Pending" },
    { id: 3, product: "Notebooks", supplier: "Ryman", scheduledDate: "2025-08-03", arrivalDate: "2025-08-05", office: "Birmingham", status: "Delayed" }
  ]);

  const [searchTerm, setSearchTerm] = useState("");
  const [filterOffice, setFilterOffice] = useState("");
  const [newDelivery, setNewDelivery] = useState({
    product: "",
    supplier: "",
    office: "",
    scheduledDate: "",
  });
  const [successMsg, setSuccessMsg] = useState("");

  const getStatusColor = (status) => {
    switch (status) {
      case "On Time":
        return "green";
      case "Pending":
        return "orange";
      case "Delayed":
        return "red";
      default:
        return "black";
    }
  };

  const handleInputChange = (e) => {
    setNewDelivery({ ...newDelivery, [e.target.name]: e.target.value });
  };

  const handleAddDelivery = (e) => {
    e.preventDefault();

    if (!newDelivery.product || !newDelivery.supplier || !newDelivery.office || !newDelivery.scheduledDate) {
      alert("Please fill in all fields.");
      return;
    }

    const newEntry = {
      id: Date.now(),
      product: newDelivery.product,
      supplier: newDelivery.supplier,
      office: newDelivery.office,
      scheduledDate: newDelivery.scheduledDate,
      arrivalDate: "",
      status: "Pending",
    };

    setDeliveries([...deliveries, newEntry]);
    setNewDelivery({ product: "", supplier: "", office: "", scheduledDate: "" });
    setSuccessMsg("Delivery added successfully!");
    setTimeout(() => setSuccessMsg(""), 3000);
  };

  const filteredDeliveries = deliveries.filter((delivery) => {
    const matchesSearch = delivery.product.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesOffice = filterOffice ? delivery.office === filterOffice : true;
    return matchesSearch && matchesOffice;
  });

  return (
    <div style={containerStyle}>
      <h2>Delivery Tracking</h2>

      {/* Search & Filter */}
      <div style={{ display: "flex", gap: "10px", marginBottom: "1rem" }}>
        <input
          type="text"
          placeholder="Search product..."
          style={inputStyle}
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
        />
        <select
          style={inputStyle}
          value={filterOffice}
          onChange={(e) => setFilterOffice(e.target.value)}
        >
          <option value="">All Offices</option>
          <option value="London">London</option>
          <option value="Manchester">Manchester</option>
          <option value="Birmingham">Birmingham</option>
        </select>
      </div>

      {/* Delivery Table */}
      <table style={{ width: "100%", borderCollapse: "collapse" }}>
        <thead>
          <tr style={{ background: "#f0f0f0" }}>
            <th style={thStyle}>Product</th>
            <th style={thStyle}>Supplier</th>
            <th style={thStyle}>Office</th>
            <th style={thStyle}>Scheduled Date</th>
            <th style={thStyle}>Arrival Date</th>
            <th style={thStyle}>Status</th>
          </tr>
        </thead>
        <tbody>
          {filteredDeliveries.length > 0 ? (
            filteredDeliveries.map((delivery) => (
              <tr key={delivery.id}>
                <td style={tdStyle}>{delivery.product}</td>
                <td style={tdStyle}>{delivery.supplier}</td>
                <td style={tdStyle}>{delivery.office}</td>
                <td style={tdStyle}>{delivery.scheduledDate}</td>
                <td style={tdStyle}>{delivery.arrivalDate || "Not Arrived"}</td>
                <td style={{ ...tdStyle, color: getStatusColor(delivery.status), fontWeight: "bold" }}>
                  {delivery.status}
                </td>
              </tr>
            ))
          ) : (
            <tr>
              <td colSpan="6" style={{ textAlign: "center", padding: "1rem" }}>No matching deliveries found</td>
            </tr>
          )}
        </tbody>
      </table>

      {/* Add New Delivery Form */}
      <form onSubmit={handleAddDelivery} style={formStyle}>
        <h3>Add New Delivery</h3>
        {successMsg && <div style={successStyle}>{successMsg}</div>}
        <input
          type="text"
          name="product"
          placeholder="Product Name"
          style={inputStyle}
          value={newDelivery.product}
          onChange={handleInputChange}
        />
        <input
          type="text"
          name="supplier"
          placeholder="Supplier Name"
          style={inputStyle}
          value={newDelivery.supplier}
          onChange={handleInputChange}
        />
        <select
          name="office"
          style={inputStyle}
          value={newDelivery.office}
          onChange={handleInputChange}
        >
          <option value="">Select Office</option>
          <option value="London">London</option>
          <option value="Manchester">Manchester</option>
          <option value="Birmingham">Birmingham</option>
        </select>
        <input
          type="date"
          name="scheduledDate"
          style={inputStyle}
          value={newDelivery.scheduledDate}
          onChange={handleInputChange}
        />
        <button type="submit" style={buttonStyle}>Add Delivery</button>
      </form>
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
  marginTop: "2rem",
  border: "1px solid #ccc",
  padding: "1rem",
  borderRadius: "8px",
  maxWidth: "400px",
};

const inputStyle = {
  padding: "8px",
  width: "100%",
  maxWidth: "300px",
  margin: "0.5rem 0",
};

const buttonStyle = {
  padding: "10px 16px",
  marginTop: "10px",
  backgroundColor: "#007bff",
  color: "#fff",
  border: "none",
  borderRadius: "4px",
  cursor: "pointer",
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

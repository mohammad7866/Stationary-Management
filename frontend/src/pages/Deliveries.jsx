// frontend/src/pages/Deliveries.jsx
import { useState } from "react";

function Deliveries() {
  const [deliveries] = useState([
    { id: 1, product: "Printer Paper A4", supplier: "Office Depot", scheduledDate: "2025-08-05", arrivalDate: "2025-08-04", office: "London", status: "On Time" },
    { id: 2, product: "Pens - Black", supplier: "Staples", scheduledDate: "2025-08-06", arrivalDate: "", office: "Manchester", status: "Pending" },
    { id: 3, product: "Notebooks", supplier: "Ryman", scheduledDate: "2025-08-03", arrivalDate: "2025-08-05", office: "Birmingham", status: "Delayed" }
  ]);

  const [searchTerm, setSearchTerm] = useState("");
  const [filterOffice, setFilterOffice] = useState("");

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

  // Filtered Deliveries
  const filteredDeliveries = deliveries.filter((delivery) => {
    const matchesSearch = delivery.product.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesOffice = filterOffice ? delivery.office === filterOffice : true;
    return matchesSearch && matchesOffice;
  });

  return (
    <div style={{ padding: "1rem" }}>
      <h2>Delivery Tracking</h2>

      {/* Search & Filter */}
      <div style={{ marginBottom: "1rem", display: "flex", gap: "10px" }}>
        <input
          type="text"
          placeholder="Search product..."
          style={{ padding: "0.5rem", flex: 1 }}
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
        />
        <select
          style={{ padding: "0.5rem" }}
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
      <table border="1" cellPadding="8" style={{ width: "100%", borderCollapse: "collapse" }}>
        <thead>
          <tr>
            <th>Product</th>
            <th>Supplier</th>
            <th>Office</th>
            <th>Scheduled Date</th>
            <th>Arrival Date</th>
            <th>Status</th>
          </tr>
        </thead>
        <tbody>
          {filteredDeliveries.length > 0 ? (
            filteredDeliveries.map((delivery) => (
              <tr key={delivery.id}>
                <td>{delivery.product}</td>
                <td>{delivery.supplier}</td>
                <td>{delivery.office}</td>
                <td>{delivery.scheduledDate}</td>
                <td>{delivery.arrivalDate || "Not Arrived"}</td>
                <td style={{ color: getStatusColor(delivery.status), fontWeight: "bold" }}>
                  {delivery.status}
                </td>
              </tr>
            ))
          ) : (
            <tr>
              <td colSpan="6" style={{ textAlign: "center" }}>No matching deliveries found</td>
            </tr>
          )}
        </tbody>
      </table>

      {/* Add New Delivery */}
      <div style={{ marginTop: "2rem" }}>
        <h3>Add New Delivery</h3>
        <form style={{ display: "grid", gap: "0.5rem", maxWidth: "400px" }}>
          <input type="text" placeholder="Product Name" />
          <input type="text" placeholder="Supplier Name" />
          <select>
            <option value="">Select Office</option>
            <option value="London">London</option>
            <option value="Manchester">Manchester</option>
            <option value="Birmingham">Birmingham</option>
          </select>
          <input type="date" placeholder="Scheduled Date" />
          <button type="submit" style={{ padding: "0.5rem", background: "blue", color: "white", border: "none" }}>
            Add Delivery
          </button>
        </form>
      </div>
    </div>
  );
}

export default Deliveries;

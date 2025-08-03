// frontend/src/App.jsx
import { BrowserRouter as Router, Routes, Route, Link } from "react-router-dom";
import Inventory from "./pages/Inventory";
import Requests from "./pages/Request";
import Deliveries from "./pages/Deliveries";
import Dashboard from "./pages/Dashboard";

function App() {
  return (
    <Router>
      <div style={{ padding: "1rem", background: "#f8f9fa" }}>
        <h1>PwC Stationery Management System</h1>

        {/* Navigation Menu */}
        <nav style={{ marginBottom: "1rem", display: "flex", gap: "1rem" }}>
          <Link to="/" style={{ textDecoration: "none", color: "blue" }}>Dashboard</Link>
          <Link to="/inventory" style={{ textDecoration: "none", color: "blue" }}>Inventory</Link>
          <Link to="/requests" style={{ textDecoration: "none", color: "blue" }}>Requests</Link>
          <Link to="/deliveries" style={{ textDecoration: "none", color: "blue" }}>Deliveries</Link>
        </nav>

        {/* Page Routes */}
        <Routes>
          <Route path="/" element={<Dashboard />} />
          <Route path="/inventory" element={<Inventory />} />
          <Route path="/requests" element={<Requests />} />
          <Route path="/deliveries" element={<Deliveries />} />
        </Routes>
      </div>
    </Router>
  );
}

export default App;

import { BrowserRouter as Router, Routes, Route, Link } from 'react-router-dom';
import Inventory from './pages/Inventory';
import Requests from './pages/Request';

function App() {
  return (
    <Router>
      <div style={{ padding: "1rem", background: "#f8f9fa" }}>
        <h1>PwC Stationery Management System</h1>

        <nav style={{ marginBottom: "1rem" }}>
          <Link to="/" style={navStyle}>Home</Link>
          <Link to="/inventory" style={navStyle}>Inventory</Link>
          <Link to="/requests" style={navStyle}>Requests</Link>
        </nav>

        <Routes>
          <Route path="/" element={<h2>Welcome to the Dashboard</h2>} />
          <Route path="/inventory" element={<Inventory />} />
          <Route path="/requests" element={<Requests />} />
        </Routes>
      </div>
    </Router>
  );
}

const navStyle = {
  marginRight: "1rem",
  textDecoration: "none",
  color: "#007bff",
};

export default App;

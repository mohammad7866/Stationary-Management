import { BrowserRouter as Router, Routes, Route, Link } from 'react-router-dom';
import Inventory from './pages/Inventory.jsx';

function App() {
  return (
    <Router>
      <div style={{ padding: "1rem", background: "#f8f9fa" }}>
        <h1>PwC Stationery Management System</h1>
        <nav style={{ marginBottom: "1rem" }}>
          <Link to="/inventory" style={{ marginRight: "1rem" }}>Inventory</Link>
        </nav>
        <Routes>
          <Route path="/inventory" element={<Inventory />} />
          <Route path="/" element={<h2>Welcome to the Dashboard</h2>} />
        </Routes>
      </div>
    </Router>
  );
}

export default App;

// src/App.jsx
import React from "react";
import { BrowserRouter as Router, Routes, Route, Link } from "react-router-dom";

import AuthProvider, { useAuth } from "./auth/AuthContext";
import ProtectedRoute from "./auth/ProtectedRoute";
import { can } from "./auth/permissions";

import LoginPage from "./pages/Login";
import Inventory from "./pages/inventory";
import Requests from "./pages/Request";
import Deliveries from "./pages/Deliveries";
import Dashboard from "./pages/Dashboard";
import Suppliers from "./pages/Suppliers";
import Forbidden from "./pages/Forbidden";
import NotFound from "./pages/NotFound";
function Nav() {
  const { token, roles = [], logout } = useAuth();

  return (
    <nav style={{ marginBottom: "1rem", display: "flex", gap: "1rem", alignItems: "center" }}>
      {can(roles, "Dashboard")  && <Link to="/">Dashboard</Link>}
      {can(roles, "Inventory")  && <Link to="/inventory">Inventory</Link>}
      {can(roles, "Requests")   && <Link to="/requests">Requests</Link>}
      {can(roles, "Deliveries") && <Link to="/deliveries">Deliveries</Link>}
      {can(roles, "Suppliers")  && <Link to="/suppliers">Suppliers</Link>}

      <div style={{flex:1}} />
      {token ? <button onClick={logout}>Logout</button> : <Link to="/login">Login</Link>}
    </nav>
  );
}

export default function App() {
  return (
    <AuthProvider>
      <Router>
        <div style={{ padding: "1rem", background: "#f8f9fa" }}>
          <h1>PwC Stationery Management System</h1>
          <Nav />

          <Routes>
            {/* public */}
            <Route path="/login" element={<LoginPage />} />

            {/* protected */}
            <Route
              path="/"
              element={
                <ProtectedRoute roles={["User","Admin","SuperAdmin"]}>
                  <Dashboard />
                </ProtectedRoute>
              }
            />
            <Route
              path="/inventory"
              element={
                <ProtectedRoute roles={["User","Admin","SuperAdmin"]}>
                  <Inventory />
                </ProtectedRoute>
              }
            />
            <Route
              path="/requests"
              element={
                <ProtectedRoute roles={["User","Admin","SuperAdmin"]}>
                  <Requests />
                </ProtectedRoute>
              }
            />
            <Route
              path="/deliveries"
              element={
                <ProtectedRoute roles={["Admin","SuperAdmin"]}>
                  <Deliveries />
                </ProtectedRoute>
              }
            />
            <Route
              path="/suppliers"
              element={
                <ProtectedRoute roles={["Admin","SuperAdmin"]}>
                  <Suppliers />
                </ProtectedRoute>
              }
            />

            {/* forbidden fallback */}
            <Route path="/403" element={<Forbidden />} />

            <Route path="*" element={<NotFound />} />

          </Routes>
        </div>
      </Router>
    </AuthProvider>
  );
}

// frontend/src/App.jsx
import React from "react";
import { BrowserRouter, Routes, Route, Link } from "react-router-dom";
import AuthProvider, { useAuth } from "./auth/AuthContext";
import ProtectedRoute from "./components/ProtectedRoute";

import LoginPage from "./pages/Login";
import SuperAdminPage from "./pages/SuperAdmin";
import AdminPage from "./pages/Admin";
import UserHome from "./pages/UserHome";

import DeliveriesPage from "./pages/Deliveries";
import RequestsPage from "./pages/Requests";
// import your other pages here (Inventory, Suppliers, etc.)

function Nav() {
  const { token, roles, logout } = useAuth();
  const isAdmin = roles.includes("Admin") || roles.includes("SuperAdmin");
  const isSuper = roles.includes("SuperAdmin");

  return (
    <nav style={{display:"flex", gap:12, padding:10, borderBottom:"1px solid #ddd"}}>
      <Link to="/">Home</Link>
      <Link to="/deliveries">Deliveries</Link>
      <Link to="/requests">Requests</Link>
      {isAdmin && <Link to="/admin">Admin</Link>}
      {isSuper && <Link to="/super-admin">Super Admin</Link>}
      <div style={{flex:1}} />
      {token ? <button onClick={logout}>Logout</button> : <Link to="/login">Login</Link>}
    </nav>
  );
}

export default function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Nav />
        <Routes>
          {/* public */}
          <Route path="/login" element={<LoginPage />} />

          {/* user routes */}
          <Route path="/" element={
            <ProtectedRoute roles={["User","Admin","SuperAdmin"]}>
              <UserHome />
            </ProtectedRoute>
          } />
          <Route path="/deliveries" element={
            <ProtectedRoute roles={["User","Admin","SuperAdmin"]}>
              <DeliveriesPage />
            </ProtectedRoute>
          } />
          <Route path="/requests" element={
            <ProtectedRoute roles={["User","Admin","SuperAdmin"]}>
              <RequestsPage />
            </ProtectedRoute>
          } />

          {/* admin */}
          <Route path="/admin" element={
            <ProtectedRoute roles={["Admin","SuperAdmin"]}>
              <AdminPage />
            </ProtectedRoute>
          } />

          {/* super admin */}
          <Route path="/super-admin" element={
            <ProtectedRoute roles={["SuperAdmin"]}>
              <SuperAdminPage />
            </ProtectedRoute>
          } />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  );
}

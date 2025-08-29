// frontend/src/components/ProtectedRoute.jsx
import React from "react";
import { Navigate } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";

export default function ProtectedRoute({ roles: needed = [], children }) {
  const { token, roles } = useAuth();
  if (!token) return <Navigate to="/login" replace />;
  if (needed.length && !needed.some(r => roles.includes(r))) return <Navigate to="/login" replace />;
  return children;
}

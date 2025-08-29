// src/auth/ProtectedRoute.jsx
import React from "react";
import { Navigate } from "react-router-dom";
import { useAuth } from "./AuthContext";

/**
 * Wrap a route element and allow only specific roles.
 * Example:
 *   <ProtectedRoute roles={["Admin","SuperAdmin"]}>
 *     <Inventory />
 *   </ProtectedRoute>
 */
export default function ProtectedRoute({ roles = [], children }) {
  const { token, roles: userRoles = [] } = useAuth();

  if (!token) return <Navigate to="/login" replace />;

  // If a roles list is provided, ensure intersection
  if (roles.length && !roles.some(r => userRoles.includes(r))) {
    return <Navigate to="/403" replace />;
  }

  return children;
}

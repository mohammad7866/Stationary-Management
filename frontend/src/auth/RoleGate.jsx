// src/auth/RoleGate.jsx
import React from "react";
import { useAuth } from "./AuthContext";
import { can } from "./permissions";

/**
 * RoleGate hides its children unless the current user is allowed
 * based on the feature name in permissions.js.
 *
 * Usage:
 *   <RoleGate feature="Inventory">
 *     <button>Adjust Stock</button>
 *   </RoleGate>
 */
export default function RoleGate({ feature, children }) {
  const { roles = [] } = useAuth();
  return can(roles, feature) ? <>{children}</> : null;
}

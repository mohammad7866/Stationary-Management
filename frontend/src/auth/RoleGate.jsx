// src/auth/RoleGate.jsx
import React from "react";
import { useAuth } from "./AuthContext";
import { can } from "./permissions";

export default function RoleGate({ feature, children, fallback = null }) {
  const { roles = [] } = useAuth();
  if (!feature) return null;
  return can(roles, feature) ? children : fallback;
}

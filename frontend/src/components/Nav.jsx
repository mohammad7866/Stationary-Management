// src/components/Nav.jsx
import React from "react";
import { NavLink } from "react-router-dom";
import { permissions, can } from "../auth/permissions";
import { useAuth } from "../auth/AuthContext";

const navItems = [
  { label: "Dashboard", path: "/dashboard", feature: "Dashboard" },
  { label: "Inventory", path: "/inventory", feature: "Inventory" },
  { label: "Approvals", path: "/approvals", feature: "Approvals" },
  { label: "Suppliers", path: "/suppliers", feature: "Suppliers" },
  { label: "Audit Logs", path: "/audit", feature: "AuditLogs" },
  { label: "Requests", path: "/requests", feature: "Requests" },
];

export default function Nav() {
  const { role } = useAuth();
  return (
    <nav>
      {navItems
        .filter(item => can(role, item.feature))
        .map(item => (
          <NavLink key={item.path} to={item.path}>
            {item.label}
          </NavLink>
        ))}
    </nav>
  );
}

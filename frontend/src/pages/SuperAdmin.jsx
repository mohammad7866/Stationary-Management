// frontend/src/pages/SuperAdmin.jsx
import React from "react";
import { useAuth } from "../auth/AuthContext";

export default function SuperAdminPage() {
  const { username, roles } = useAuth();
  return (
    <div style={{padding:20}}>
      <h1>Super Admin</h1>
      <p>Welcome {username}. Roles: {roles.join(", ")}</p>
      <ul>
        <li>Manage users & roles</li>
        <li>Delete deliveries/records</li>
        <li>System settings</li>
      </ul>
    </div>
  );
}

// frontend/src/pages/Admin.jsx
import React from "react";
import { useAuth } from "../auth/AuthContext";

export default function AdminPage() {
  const { username, roles } = useAuth();
  return (
    <div style={{padding:20}}>
      <h1>Admin</h1>
      <p>Welcome {username}. Roles: {roles.join(", ")}</p>
      <ul>
        <li>Create/Update deliveries, items, suppliers</li>
        <li>Approve/Reject requests</li>
      </ul>
    </div>
  );
}

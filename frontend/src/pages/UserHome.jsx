// frontend/src/pages/UserHome.jsx
import React from "react";
import { useAuth } from "../auth/AuthContext";

export default function UserHome() {
  const { username, roles } = useAuth();
  return (
    <div style={{padding:20}}>
      <h1>User Home</h1>
      <p>Welcome {username}. Roles: {roles.join(", ")}</p>
      <ul>
        <li>Submit requests</li>
        <li>View deliveries</li>
      </ul>
    </div>
  );
}

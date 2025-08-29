// src/auth/permissions.js
// ES module exports (what Vite expects)

export const permissions = {
  Dashboard:  ["User", "Admin", "SuperAdmin"],
  Inventory:  ["Admin", "SuperAdmin"],
  Requests:   ["User", "Admin", "SuperAdmin"],
  Deliveries: ["Admin", "SuperAdmin"],
  Suppliers:  ["Admin", "SuperAdmin"],
};

export function can(roleList = [], feature) {
  const allowed = permissions[feature] || [];
  return Array.isArray(roleList) && roleList.some(r => allowed.includes(r));
}

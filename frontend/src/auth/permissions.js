// src/auth/permissions.js

import { AuditLogs } from "../lib/api";

// Pages (visibility in nav/routes)
export const permissions = {
  Dashboard:       ["User", "Admin", "SuperAdmin"],
  Inventory:       ["User", "Admin", "SuperAdmin"],  // Users can view inventory
  Requests:        ["User", "Admin", "SuperAdmin"],
  Deliveries:      ["Admin", "SuperAdmin"],
  Suppliers:       ["Admin", "SuperAdmin"],
  AuditLog:        ["SuperAdmin"],
  // Fine-grained actions (buttons/controls)
  InventoryManage: ["Admin", "SuperAdmin"],          // edit, adjust, add
  InventoryDelete: ["SuperAdmin"],                   // delete item/row
  RequestsApprove: ["Admin", "SuperAdmin"],          // approve/reject
  RequestsDelete:  ["SuperAdmin"],  
  DeliveriesCreate: ["Admin","SuperAdmin"],
};
                 // delete request

export function can(roleList = [], feature) {
  const allowed = permissions[feature] || [];
  return Array.isArray(roleList) && roleList.some(r => allowed.includes(r));
}

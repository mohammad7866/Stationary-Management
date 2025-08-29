// frontend/src/lib/api.js
const BASE = import.meta.env.VITE_API_BASE_URL || "";

function toQuery(obj) {
  if (!obj) return "";
  const p = new URLSearchParams();
  for (const [k, v] of Object.entries(obj)) {
    if (v === null || v === undefined || v === "") continue;
    p.append(k, String(v));
  }
  const q = p.toString();
  return q ? `?${q}` : "";
}

function http(path, opts = {}) {
  // Align with AuthContext: use the "token" key
  const token = localStorage.getItem("token") || "";
  const headers = {
    "Content-Type": "application/json",
    ...(opts.headers || {}),
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
  };
  return fetch(`${BASE}${path}`, { ...opts, headers }).then(async (r) => {
    const text = await r.text();
    const data = text ? (() => { try { return JSON.parse(text); } catch { return text; } })() : null;
    if (!r.ok) {
      const msg =
        (data && (data.title || data.error || data.message)) ||
        text ||
        `HTTP ${r.status}`;
      const err = new Error(msg);
      err.status = r.status;
      err.body = data;
      throw err;
    }
    return data;
  });
}

/* ===== Auth ===== */
export const Auth = {
  login: (body) => http(`/api/Auth/login`, { method: "POST", body: JSON.stringify(body) }),
  me: (token) =>
  fetch(`${BASE}/api/Auth/me`, {
    headers: { Authorization: `Bearer ${token}` },
  }).then((r) => r.json()),

};

/* ===== Items ===== */
export const Items = {
  list: (p) => http(`/api/Items${toQuery(p)}`),
  get: (id) => http(`/api/Items/${id}`),
  create: (data) => http(`/api/Items`, { method: "POST", body: JSON.stringify(data) }),
  update: (id, data) => http(`/api/Items/${id}`, { method: "PUT", body: JSON.stringify(data) }),
  remove: (id) => http(`/api/Items/${id}`, { method: "DELETE" }),
};

/* ===== Suppliers ===== */
export const Suppliers = {
  list: (p) => http(`/api/Suppliers${toQuery(p)}`),
  get: (id) => http(`/api/Suppliers/${id}`),
  create: (data) => http(`/api/Suppliers`, { method: "POST", body: JSON.stringify(data) }),
  update: (id, data) => http(`/api/Suppliers/${id}`, { method: "PUT", body: JSON.stringify(data) }),
  remove: (id) => http(`/api/Suppliers/${id}`, { method: "DELETE" }),
};

/* ===== Offices ===== */
export const Offices = {
  list: (p) => http(`/api/Offices${toQuery(p)}`),
  get: (id) => http(`/api/Offices/${id}`),
  create: (data) => http(`/api/Offices`, { method: "POST", body: JSON.stringify(data) }),
  update: (id, data) => http(`/api/Offices/${id}`, { method: "PUT", body: JSON.stringify(data) }),
  remove: (id) => http(`/api/Offices/${id}`, { method: "DELETE" }),
};

/* ===== Categories ===== */
export const Categories = {
  list: (p) => http(`/api/Categories${toQuery(p)}`),
  get: (id) => http(`/api/Categories/${id}`),
  create: (data) => http(`/api/Categories`, { method: "POST", body: JSON.stringify(data) }),
  update: (id, data) => http(`/api/Categories/${id}`, { method: "PUT", body: JSON.stringify(data) }),
  remove: (id) => http(`/api/Categories/${id}`, { method: "DELETE" }),
};

/* ===== Stock Levels ===== */
export const StockLevels = {
  list: (p) => http(`/api/StockLevels${toQuery(p)}`),
  get: (id) => http(`/api/StockLevels/${id}`),
  create: (data) => http(`/api/StockLevels`, { method: "POST", body: JSON.stringify(data) }),
  update: (id, data) => http(`/api/StockLevels/${id}`, { method: "PUT", body: JSON.stringify(data) }),
  adjust: (id, delta) => http(`/api/StockLevels/${id}/adjust`, { method: "POST", body: JSON.stringify(delta) }),
  remove: (id) => http(`/api/StockLevels/${id}`, { method: "DELETE" }),
};

/* ===== Deliveries ===== */
export const Deliveries = {
  list: (p) => http(`/api/Deliveries${toQuery(p)}`),
  get: (id) => http(`/api/Deliveries/${id}`),
  create: (data) => http(`/api/Deliveries`, { method: "POST", body: JSON.stringify(data) }),
  update: (id, data) => http(`/api/Deliveries/${id}`, { method: "PUT", body: JSON.stringify(data) }),
  updateStatus: (id, data) => http(`/api/Deliveries/${id}/status`, { method: "POST", body: JSON.stringify(data) }),
  remove: (id) => http(`/api/Deliveries/${id}`, { method: "DELETE" }),
};

/* ===== Requests ===== */
export const Requests = {
  list: (p) => http(`/api/Requests${toQuery(p)}`),
  get: (id) => http(`/api/Requests/${id}`),
  create: (data) => http(`/api/Requests`, { method: "POST", body: JSON.stringify(data) }),
  update: (id, data) => http(`/api/Requests/${id}`, { method: "PUT", body: JSON.stringify(data) }),
  setStatus: (id, data) => http(`/api/Requests/${id}/status`, { method: "POST", body: JSON.stringify(data) }),
  approve: (id) => http(`/api/Requests/${id}/approve`, { method: "POST" }),
  reject: (id) => http(`/api/Requests/${id}/reject`, { method: "POST" }),
  remove: (id) => http(`/api/Requests/${id}`, { method: "DELETE" }),
};

/* ===== named utils (and http) ===== */
export { http, toQuery, BASE };

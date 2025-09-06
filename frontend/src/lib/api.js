// frontend/src/lib/api.js

let AUTH_TOKEN = ""; // in-memory source of truth (prevents timing races)

export function setAuthToken(t) {
  AUTH_TOKEN = t || "";
  try {
    if (AUTH_TOKEN) localStorage.setItem("token", AUTH_TOKEN);
    else localStorage.removeItem("token");
  } catch {}
}

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

function safeParse(text) {
  try { return JSON.parse(text); } catch { return text; }
}

function http(path, opts = {}) {
  // Prefer in-memory token; fall back to localStorage (e.g., on hard refresh before context mounts)
  const token = AUTH_TOKEN || localStorage.getItem("token") || "";

  const headers = {
    "Content-Type": "application/json",
    ...(opts.headers || {}),
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
  };

  return fetch(`${BASE}${path}`, { ...opts, headers }).then(async (r) => {
    const text = await r.text();
    const data = text ? safeParse(text) : null;

    if (!r.ok) {
      // ❌ Do NOT auto-clear token here — a single early 401 can happen during login races
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

  adjust: (id, deltaOrBody, maybeReason) => {
    let payload;
    if (typeof deltaOrBody === "object" && deltaOrBody !== null) {
      payload = deltaOrBody;
    } else {
      const num = Number(deltaOrBody);
      payload = (maybeReason === undefined || maybeReason === null)
        ? num
        : { delta: num, reason: maybeReason };
    }
    return http(`/api/StockLevels/${id}/adjust`, {
      method: "POST",
      body: JSON.stringify(payload),
    });
  },

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

/* ===== Audit Logs ===== */
export const AuditLogs = {
  list: (p) => http(`/api/AuditLogs${toQuery(p)}`),
};

/* ===== Issues ===== */
export const Issues = {
  create: (data) => http(`/api/Issues`, { method: "POST", body: JSON.stringify(data) }),
  get:    (id)    => http(`/api/Issues/${id}`),
  byRequest: (requestId) => http(`/api/Issues/by-request/${requestId}`),
};

/* ===== Returns ===== */
export const Returns = {
  create:   (data)      => http(`/api/Returns`, { method: "POST", body: JSON.stringify(data) }),
  byIssue:  (issueId)   => http(`/api/Returns/by-issue/${issueId}`),
};

/* ===== Replenishment ===== */
export const Replenishment = {
  suggestions: (params) => http(`/api/Replenishment/suggestions${toQuery(params)}`),
  raise: (data) => http(`/api/Replenishment/raise`, { method: "POST", body: JSON.stringify(data) }),
};

export { http, toQuery, BASE };

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
  const token = localStorage.getItem("token") || "";
  const headers = {
    "Content-Type": "application/json",
    ...(opts.headers || {}),
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
  };

  return fetch(`${BASE}${path}`, { ...opts, headers }).then(async (r) => {
    // Handle unauthorized early
    if (r.status === 401) {
      try { localStorage.removeItem("token"); } catch {}
      // optional: broadcast logout if you want other tabs to react
      // window.dispatchEvent(new Event("auth:logout"));
      throw new Error("Unauthorized");
    }

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

  // Sends a raw number when no reason is passed (matches Swagger),
  // and sends { delta, reason } when a reason is provided or an object is passed in.
  adjust: (id, deltaOrBody, maybeReason) => {
    let payload;
    if (typeof deltaOrBody === "object" && deltaOrBody !== null) {
      // e.g. { delta: -6, reason: "reconcile" }
      payload = deltaOrBody;
    } else {
      const num = Number(deltaOrBody);
      payload = (maybeReason === undefined || maybeReason === null)
        ? num                    // 🔹 raw number (what Swagger sends)
        : { delta: num, reason: maybeReason }; // object if a reason is supplied
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

/* ===== named utils (and http) ===== */
export { http, toQuery, BASE };

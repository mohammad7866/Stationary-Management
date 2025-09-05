import { useEffect, useMemo, useState } from "react";
import { AuditLogs } from "../lib/api";

// --- utils ---
function formatWhen(ts) {
  if (!ts && ts !== 0) return "—";
  const d = new Date(ts);
  return isNaN(d.getTime()) ? "—" : d.toLocaleString();
}
function shortActor(s) {
  if (!s) return "—";
  const at = s.indexOf("@");
  return at > 0 ? s.slice(0, at) : s;
}
function Chip({ active, onClick, children }) {
  return (
    <button
      type="button"
      onClick={onClick}
      className={
        "px-2 py-1 rounded border text-sm transition " +
        (active ? "bg-black text-white border-black" : "bg-white hover:bg-gray-50")
      }
    >
      {children}
    </button>
  );
}
function downloadCsv(filename, rows) {
  const header = ["Timestamp", "Actor", "Action", "Entity", "Details"];
  const lines = [header, ...rows.map(r => [
    formatWhen(r.timestamp),
    shortActor(r.actorName ?? r.actorId),
    r.action || "",
    `${r.entityType || ""}${r.entityId ? ` #${r.entityId}` : ""}`.trim(),
    (typeof r.details === "string" ? r.details : JSON.stringify(r.details ?? "")) || ""
  ])];

  const csv = lines
    .map(cols => cols.map(v => {
      const s = String(v ?? "");
      if (/[",\n]/.test(s)) return `"${s.replace(/"/g, '""')}"`;
      return s;
    }).join(","))
    .join("\n");

  const blob = new Blob([csv], { type: "text/csv;charset=utf-8" });
  const url = URL.createObjectURL(blob);
  const a = document.createElement("a");
  a.href = url;
  a.download = filename;
  document.body.appendChild(a);
  a.click();
  URL.revokeObjectURL(url);
  a.remove();
}

export default function AuditLog() {
  // data
  const [rows, setRows] = useState([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const pageSize = 25;
  const [loading, setLoading] = useState(true);
  const [err, setErr] = useState("");

  // filters / toggles
  const [showFilters, setShowFilters] = useState(false);
  const [search, setSearch] = useState("");
  const [from, setFrom] = useState(""); // yyyy-mm-dd
  const [to, setTo] = useState("");
  const [includeReads, setIncludeReads] = useState(false);
  const [includeSelf, setIncludeSelf] = useState(false);
  const [includeAuth, setIncludeAuth] = useState(false);
  const [showUrl, setShowUrl] = useState(false);

  // chips (server-side CSV params)
  const [selectedActions, setSelectedActions] = useState(new Set());
  const [selectedEntities, setSelectedEntities] = useState(new Set());

  // sorting
  const [sortBy, setSortBy] = useState("timestamp"); // timestamp | actor | action | entity
  const [sortDir, setSortDir] = useState("desc");    // asc | desc

  const canPrev = page > 1;
  const canNext = page * pageSize < total;

  // suggestions from current page + sensible defaults
  const suggestions = useMemo(() => {
    const actionCounts = new Map();
    const entityCounts = new Map();
    for (const r of rows) {
      const act = (r.action || "").trim();
      const ent = (r.entityType || "").trim();
      if (act) actionCounts.set(act, (actionCounts.get(act) || 0) + 1);
      if (ent) entityCounts.set(ent, (entityCounts.get(ent) || 0) + 1);
    }
    const defaultActions = ["Created", "Updated", "Deleted", "Approved", "Rejected", "Issued", "Returned", "Adjusted", "Viewed"];
    const defaultEntities = ["Item", "StockLevel", "Request", "Delivery", "Supplier", "Office", "Category", "Auth", "AuditLogs"];
    for (const a of defaultActions) if (!actionCounts.has(a)) actionCounts.set(a, 0);
    for (const e of defaultEntities) if (!entityCounts.has(e)) entityCounts.set(e, 0);

    const sortMap = (m) => [...m.entries()].sort((a,b) => b[1]-a[1] || a[0].localeCompare(b[0])).map(([k]) => k);
    return {
      actions: sortMap(actionCounts).slice(0, 10),
      entities: sortMap(entityCounts).slice(0, 12)
    };
  }, [rows]);

  const toggleAction = (a) => {
    const next = new Set(selectedActions);
    next.has(a) ? next.delete(a) : next.add(a);
    setSelectedActions(next);
  };
  const toggleEntity = (e) => {
    const next = new Set(selectedEntities);
    next.has(e) ? next.delete(e) : next.add(e);
    setSelectedEntities(next);
  };

  async function load(signal) {
    setLoading(true);
    setErr("");
    try {
      const params = {
        page,
        pageSize,
        includeReads,
        includeSelf,
        includeAuth,
        showUrl,
        sortBy,
        sortDir
      };
      if (search.trim()) params.search = search.trim();
      if (from) params.from = new Date(from).toISOString();
      if (to) params.to = new Date(to).toISOString();
      if (selectedActions.size) params.actions = [...selectedActions].join(",");
      if (selectedEntities.size) params.entities = [...selectedEntities].join(",");

      const data = await AuditLogs.list(params, { signal });
      const items = Array.isArray(data?.items) ? data.items : Array.isArray(data) ? data : [];
      const count = Number.isFinite(data?.total) ? data.total : items.length;
      setRows(items);
      setTotal(count);
    } catch (e) {
      if (signal?.aborted) return;
      const msg =
        e?.status === 401 ? "You are not logged in."
        : e?.status === 403 ? "You don't have permission to view audit logs."
        : e?.message?.includes("Failed to fetch") ? "Cannot reach the API."
        : e?.message || "Failed to load audit logs.";
      setErr(msg);
    } finally {
      if (!signal?.aborted) setLoading(false);
    }
  }

  // base reloads
  useEffect(() => {
    const abort = new AbortController();
    load(abort.signal);
    return () => abort.abort();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [page, includeReads, includeSelf, includeAuth, showUrl, sortBy, sortDir]);

  // filters -> reset to page 1 and reload
  useEffect(() => {
    const abort = new AbortController();
    setPage(1);
    load(abort.signal);
    return () => abort.abort();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [search, from, to, selectedActions, selectedEntities]);

  const requestExport = () => {
    downloadCsv(`audit-logs-page${page}.csv`, rows);
  };

  const clickSort = (key) => {
    if (sortBy === key) {
      setSortDir(d => (d === "asc" ? "desc" : "asc"));
    } else {
      setSortBy(key);
      setSortDir("desc");
    }
  };

  const SortHeader = ({ label, col }) => (
    <button
      onClick={() => clickSort(col)}
      className="flex items-center gap-1 font-medium"
      title="Sort"
    >
      {label}
      <span className="text-xs opacity-70">
        {sortBy === col ? (sortDir === "asc" ? "▲" : "▼") : "▾"}
      </span>
    </button>
  );

  return (
    <div className="p-6 space-y-5 max-w-6xl mx-auto">
      <div className="flex items-center justify-between flex-wrap gap-2">
        <h1 className="text-3xl font-extrabold tracking-tight">Audit Logs</h1>
        <div className="flex items-center gap-2">
          <button
            onClick={() => setShowFilters(s => !s)}
            className="px-3 py-1.5 border rounded-lg"
          >
            {showFilters ? "Hide Filters" : "Show Filters"}
          </button>
          <button
            onClick={() => requestExport()}
            className="px-3 py-1.5 border rounded-lg"
            disabled={loading || rows.length === 0}
          >
            Export CSV
          </button>
          <button
            onClick={() => load()}
            className="px-3 py-1.5 border rounded-lg"
            disabled={loading}
          >
            {loading ? "Loading…" : "Refresh"}
          </button>
        </div>
      </div>

      {showFilters && (
        <div className="border rounded-xl p-4 space-y-4 bg-gray-50">
          <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-4 items-end">
            <div className="flex-1">
              <label className="block text-sm mb-1">Search</label>
              <input
                className="w-full border rounded px-2 py-2"
                placeholder="user, action, entity, ID…"
                value={search}
                onChange={(e) => setSearch(e.target.value)}
              />
            </div>
            <div>
              <label className="block text-sm mb-1">From</label>
              <input
                type="date"
                className="border rounded px-2 py-2"
                value={from}
                onChange={(e) => setFrom(e.target.value)}
              />
            </div>
            <div>
              <label className="block text-sm mb-1">To</label>
              <input
                type="date"
                className="border rounded px-2 py-2"
                value={to}
                onChange={(e) => setTo(e.target.value)}
              />
            </div>
          </div>

          <div className="space-y-1">
            <div className="text-sm text-gray-700">Quick Actions</div>
            <div className="flex flex-wrap gap-2">
              {suggestions.actions.map((a) => (
                <Chip key={a} active={selectedActions.has(a)} onClick={() => toggleAction(a)}>
                  {a}
                </Chip>
              ))}
            </div>
          </div>

          <div className="space-y-1">
            <div className="text-sm text-gray-700">Quick Entities</div>
            <div className="flex flex-wrap gap-2">
              {suggestions.entities.map((e) => (
                <Chip key={e} active={selectedEntities.has(e)} onClick={() => toggleEntity(e)}>
                  {e}
                </Chip>
              ))}
            </div>
          </div>

          <div className="flex flex-wrap gap-4 pt-1">
            <label className="flex items-center gap-2 text-sm">
              <input type="checkbox" checked={includeReads} onChange={(e) => setIncludeReads(e.target.checked)} />
              Include reads
            </label>
            <label className="flex items-center gap-2 text-sm">
              <input type="checkbox" checked={includeSelf} onChange={(e) => setIncludeSelf(e.target.checked)} />
              Include self-logs
            </label>
            <label className="flex items-center gap-2 text-sm">
              <input type="checkbox" checked={includeAuth} onChange={(e) => setIncludeAuth(e.target.checked)} />
              Include auth
            </label>
            <label className="flex items-center gap-2 text-sm">
              <input type="checkbox" checked={showUrl} onChange={(e) => setShowUrl(e.target.checked)} />
              Show URL details
            </label>
          </div>
        </div>
      )}

      {err && <div className="p-3 border border-red-300 bg-red-50 text-red-700 rounded">{err}</div>}
      {!err && loading && <div className="p-3">Loading audit logs…</div>}
      {!err && !loading && rows.length === 0 && <div className="p-3">No audit entries found.</div>}

      {!err && !loading && rows.length > 0 && (
        <>
          <div className="rounded-2xl border bg-white shadow-sm overflow-auto">
            <table className="min-w-full table-auto leading-relaxed">
              <thead>
                <tr className="text-left bg-gray-50 sticky top-0">
                  <th className="p-3 border-b whitespace-nowrap"><SortHeader label="Timestamp" col="timestamp" /></th>
                  <th className="p-3 border-b whitespace-nowrap"><SortHeader label="Actor" col="actor" /></th>
                  <th className="p-3 border-b whitespace-nowrap"><SortHeader label="Action" col="action" /></th>
                  <th className="p-3 border-b whitespace-nowrap"><SortHeader label="Entity" col="entity" /></th>
                  <th className="p-3 border-b w-1/2">Details</th>
                </tr>
              </thead>
              <tbody>
                {rows.map((r, i) => (
                  <tr key={r.id} className={i % 2 ? "bg-gray-50/40" : ""}>
                    <td className="p-3 border-b whitespace-nowrap">{formatWhen(r.timestamp)}</td>
                    <td className="p-3 border-b whitespace-nowrap">{shortActor(r.actorName) ?? shortActor(r.actorId)}</td>
                    <td className="p-3 border-b whitespace-nowrap">{r.action || "—"}</td>
                    <td className="p-3 border-b whitespace-nowrap">
                      {(r.entityType || "—")}{r.entityId ? ` #${r.entityId}` : ""}
                    </td>
                    <td className="p-3 border-b align-top">
                      <div className="max-w-[60ch] whitespace-normal break-words">
                        {typeof r.details === "string" ? r.details : JSON.stringify(r.details ?? "")}
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          <div className="flex items-center gap-3 pt-2">
            <button
              disabled={!canPrev}
              onClick={() => setPage((p) => Math.max(1, p - 1))}
              className="px-3 py-1.5 border rounded-lg disabled:opacity-50"
            >
              Prev
            </button>
            <span>Page {page} · {total} total</span>
            <button
              disabled={!canNext}
              onClick={() => setPage((p) => p + 1)}
              className="px-3 py-1.5 border rounded-lg disabled:opacity-50"
            >
              Next
            </button>
          </div>
        </>
      )}
    </div>
  );
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Web;
using PwCStationeryAPI.Data;
using PwCStationeryAPI.Filters;
using PwCStationeryAPI.Models.DTOs;

namespace PwCStationeryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AuditLogsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public AuditLogsController(ApplicationDbContext db) => _db = db;

        [HttpGet]
        [SkipAudit]
        public async Task<IActionResult> Get(
            [FromQuery] int skip = 0,
            [FromQuery] int take = 25,
            [FromQuery] int? page = null,
            [FromQuery] int? pageSize = null,
            [FromQuery] string? search = null,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null,
            [FromQuery] string? actions = null,   // CSV: Created,Updated,Deleted,Approved,Rejected,Issued,Returned,Adjusted,Viewed
            [FromQuery] string? entities = null,  // CSV: Item,StockLevel,Request,Delivery,Supplier,Office,Category,Auth,AuditLogs
            [FromQuery] string? sortBy = "timestamp", // timestamp|actor|action|entity
            [FromQuery] string? sortDir = "desc",      // asc|desc
            [FromQuery] bool includeReads = false,
            [FromQuery] bool includeSelf = false,
            [FromQuery] bool includeAuth = false,
            [FromQuery] bool showUrl = false)
        {
            // Paging
            if (page.HasValue && pageSize.HasValue)
            {
                var p = Math.Max(1, page.Value);
                var ps = Math.Clamp(pageSize.Value, 1, 200);
                skip = (p - 1) * ps;
                take = ps;
            }
            else
            {
                skip = Math.Max(0, skip);
                take = (take is < 1 or > 200) ? 25 : take;
            }

            var q = _db.AuditLogs.AsNoTracking();

            // Time window (support either TimestampUtc or WhenUtc)
            if (from.HasValue) q = q.Where(a => a.TimestampUtc >= from.Value || a.WhenUtc >= from.Value);
            if (to.HasValue) q = q.Where(a => a.TimestampUtc <= to.Value || a.WhenUtc <= to.Value);

            // Hide self/auth noise unless requested
            if (!includeSelf) q = q.Where(a => a.Controller != "AuditLogs");
            if (!includeAuth) q = q.Where(a => a.Controller != "Auth" && a.Entity != "Auth");

            // -------------------------
            // Parse typed search tokens:
            //   action:<name>  entity:<name>  actor:<local-part>  id:<value>
            // Remaining plain text goes to broad Contains.
            // -------------------------
            var tokenActions = new List<string>();
            var tokenEntities = new List<string>();
            string? tokenActor = null;
            string? tokenId = null;
            string? freeSearch = null;

            if (!string.IsNullOrWhiteSpace(search))
            {
                var parts = search.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                foreach (var raw in parts)
                {
                    var s = raw.Trim();
                    var idx = s.IndexOf(':');
                    if (idx > 0)
                    {
                        var key = s[..idx].ToLowerInvariant();
                        var val = s[(idx + 1)..].Trim();
                        if (string.IsNullOrEmpty(val)) continue;

                        switch (key)
                        {
                            case "action": tokenActions.Add(val); break;
                            case "entity": tokenEntities.Add(val); break;
                            case "actor": tokenActor = val; break;   // match UserName/UserId contains (case-insensitive)
                            case "id": tokenId = val; break;      // matches EntityId
                            default: freeSearch = (freeSearch == null) ? s : (freeSearch + " " + s); break;
                        }
                    }
                    else
                    {
                        freeSearch = (freeSearch == null) ? s : (freeSearch + " " + s);
                    }
                }
            }

            // Broad search (case-insensitive)
            if (!string.IsNullOrWhiteSpace(freeSearch))
            {
                var s = freeSearch;
                q = q.Where(a =>
                    (a.UserName != null && a.UserName.ToLower().Contains(s.ToLower())) ||
                    (a.UserId != null && a.UserId.ToLower().Contains(s.ToLower())) ||
                    (a.Action != null && a.Action.ToLower().Contains(s.ToLower())) ||
                    (a.Entity != null && a.Entity.ToLower().Contains(s.ToLower())) ||
                    (a.Path != null && a.Path.ToLower().Contains(s.ToLower())) ||
                    (a.QueryString != null && a.QueryString.ToLower().Contains(s.ToLower())) ||
                    (a.EntityId != null && a.EntityId.ToLower().Contains(s.ToLower()))
                );
            }

            // ACTIONS filter (chips + tokens). Case-insensitive, OR within actions.
            // Note: 'Viewed' must override the default "hide reads".
            var wantsViewed = false;
            var actionSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            void AddActionsFromCsv(string? csv)
            {
                if (string.IsNullOrWhiteSpace(csv)) return;
                foreach (var t in csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                    actionSet.Add(t);
            }
            AddActionsFromCsv(actions);
            foreach (var t in tokenActions) actionSet.Add(t);

            if (actionSet.Count > 0)
            {
                bool wantsCreated = actionSet.Contains("Created");
                bool wantsUpdated = actionSet.Contains("Updated");
                bool wantsDeleted = actionSet.Contains("Deleted");
                bool wantsApproved = actionSet.Contains("Approved");
                bool wantsRejected = actionSet.Contains("Rejected");
                bool wantsIssued = actionSet.Contains("Issued");
                bool wantsReturned = actionSet.Contains("Returned");
                bool wantsAdjusted = actionSet.Contains("Adjusted");
                wantsViewed = actionSet.Contains("Viewed");

                q = q.Where(a =>
                    // Created
                    (wantsCreated && ((a.Method != null && a.Method.ToUpper() == "POST") ||
                                       (a.Action != null && a.Action.ToLower() == "create"))) ||

                    // Updated (PUT/PATCH or explicit Update/Adjust)
                    (wantsUpdated && ((a.Method != null && (a.Method.ToUpper() == "PUT" || a.Method.ToUpper() == "PATCH")) ||
                                       (a.Action != null && (a.Action.ToLower() == "update" || a.Action.ToLower().Contains("adjust"))))) ||

                    // Deleted
                    (wantsDeleted && ((a.Method != null && a.Method.ToUpper() == "DELETE") ||
                                       (a.Action != null && a.Action.ToLower() == "delete"))) ||

                    // Approved / Rejected / Issued / Returned
                    (wantsApproved && (a.Action != null && a.Action.ToLower() == "approve")) ||
                    (wantsRejected && (a.Action != null && a.Action.ToLower() == "reject")) ||
                    (wantsIssued && (a.Action != null && a.Action.ToLower() == "issue")) ||
                    (wantsReturned && (a.Action != null && a.Action.ToLower() == "return")) ||

                    // Adjusted (handles dotted like "StockLevel.Adjust")
                    (wantsAdjusted && (a.Action != null && (a.Action.ToLower() == "adjust" || a.Action.ToLower().EndsWith(".adjust")))) ||

                    // Viewed (GET/List/GetAll)
                    (wantsViewed && ((a.Method != null && a.Method.ToUpper() == "GET") ||
                                       (a.Action != null && (a.Action.ToLower() == "list" || a.Action.ToLower() == "getall"))))
                );
            }

            // Hide reads unless explicitly requested or 'Viewed' is among selected actions
            if (!includeReads && !wantsViewed)
            {
                q = q.Where(a => !(
                    (a.Method != null && a.Method.ToUpper() == "GET") ||
                    (a.Action != null && (a.Action == "List" || a.Action == "GetAll"))
                ));
            }

            // ENTITIES filter (chips + tokens). Case-insensitive, OR within entities.
            var entitySet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            void AddEntitiesFromCsv(string? csv)
            {
                if (string.IsNullOrWhiteSpace(csv)) return;
                foreach (var t in csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                    entitySet.Add(t);
            }
            AddEntitiesFromCsv(entities);
            foreach (var t in tokenEntities) entitySet.Add(t);

            if (entitySet.Count > 0)
            {
                // Precompute lowercased wanted plus singularized variants (e.g., "Items" → "Item")
                var wantedLower = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var e in entitySet)
                {
                    var el = e.Trim();
                    if (string.IsNullOrEmpty(el)) continue;
                    wantedLower.Add(el);
                    if (el.EndsWith("s", StringComparison.OrdinalIgnoreCase))
                        wantedLower.Add(el.Substring(0, el.Length - 1));
                }

                q = q.Where(a =>
                    (a.Entity != null && (
                        wantedLower.Contains(a.Entity) ||
                        (a.Entity.EndsWith("s") && wantedLower.Contains(a.Entity.Substring(0, a.Entity.Length - 1)))
                    )) ||
                    (a.Controller != null && (
                        wantedLower.Contains(a.Controller) ||
                        (a.Controller.EndsWith("s") && wantedLower.Contains(a.Controller.Substring(0, a.Controller.Length - 1)))
                    ))
                );
            }

            // Actor token (matches either username or userId, case-insensitive)
            if (!string.IsNullOrWhiteSpace(tokenActor))
            {
                var asearch = tokenActor.ToLower();
                q = q.Where(a =>
                    (a.UserName != null && a.UserName.ToLower().Contains(asearch)) ||
                    (a.UserId != null && a.UserId.ToLower().Contains(asearch))
                );
            }

            // id token (matches EntityId)
            if (!string.IsNullOrWhiteSpace(tokenId))
            {
                var idsearch = tokenId.ToLower();
                q = q.Where(a => a.EntityId != null && a.EntityId.ToLower().Contains(idsearch));
            }

            // Count AFTER all filters
            var total = await q.CountAsync();

            // Sorting
            bool asc = string.Equals(sortDir, "asc", StringComparison.OrdinalIgnoreCase);
            var sortKey = (sortBy ?? "timestamp").ToLowerInvariant();

            q = sortKey switch
            {
                "actor" => asc ? q.OrderBy(a => a.UserName ?? a.UserId) : q.OrderByDescending(a => a.UserName ?? a.UserId),
                "action" => asc ? q.OrderBy(a => a.Action ?? a.Method) : q.OrderByDescending(a => a.Action ?? a.Method),
                "entity" => asc ? q.OrderBy(a => a.Entity ?? a.Controller) : q.OrderByDescending(a => a.Entity ?? a.Controller),
                _ => asc
                            ? q.OrderBy(a => (a.TimestampUtc > DateTime.MinValue ? a.TimestampUtc : a.WhenUtc))
                            : q.OrderByDescending(a => (a.TimestampUtc > DateTime.MinValue ? a.TimestampUtc : a.WhenUtc))
            };

            // Page slice
            var slice = await q.Skip(skip).Take(take).ToListAsync();

            // Map -> DTO
            var items = slice.Select(a =>
            {
                var ts = a.TimestampUtc.Year > 2000 ? a.TimestampUtc
                       : a.WhenUtc.Year > 2000 ? a.WhenUtc
                       : a.TimestampUtc;

                var (canonAction, entityLabel) = Canonicalize(a.Method, a.Action, a.Entity, a.Controller);
                var actorName = string.IsNullOrWhiteSpace(a.UserName)
                                ? (string.IsNullOrWhiteSpace(a.UserId) ? "System" : a.UserId)
                                : a.UserName;

                return new AuditLogDto
                {
                    Id = a.Id,
                    Timestamp = ts,
                    ActorId = a.UserId,
                    ActorName = actorName,
                    Action = canonAction,
                    EntityType = entityLabel,
                    EntityId = a.EntityId,
                    Details = BuildDetails(a, canonAction, entityLabel, showUrl)
                };
            }).ToList();

            return Ok(new { total, items });
        }

        // ---------- helpers ----------

        private static (string action, string entity) Canonicalize(string? method, string? rawAction, string? rawEntity, string? controller)
        {
            // Handle dotted actions like "StockLevel.Adjust"
            string? actionPart = rawAction;
            if (!string.IsNullOrWhiteSpace(rawAction) && rawAction.Contains('.'))
            {
                var bits = rawAction.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (bits.Length >= 2)
                {
                    rawEntity = rawEntity ?? bits[0];
                    actionPart = bits[bits.Length - 1];
                }
            }

            string action = actionPart?.ToLowerInvariant() switch
            {
                "approve" => "Approved",
                "reject" => "Rejected",
                "issue" => "Issued",
                "return" => "Returned",
                "create" => "Created",
                "update" => "Updated",
                "delete" => "Deleted",
                "adjust" => "Adjusted",
                _ => method?.ToUpperInvariant() switch
                {
                    "POST" => "Created",
                    "PUT" => "Updated",
                    "PATCH" => "Updated",
                    "DELETE" => "Deleted",
                    "GET" => "Viewed",
                    _ => string.IsNullOrWhiteSpace(rawAction) ? (method ?? "Action") : rawAction
                }
            };

            var entity = rawEntity ?? controller ?? "Resource";
            if (string.Equals(entity, "StockLevels", StringComparison.OrdinalIgnoreCase)) entity = "StockLevel";
            if (string.Equals(entity, "Requests", StringComparison.OrdinalIgnoreCase)) entity = "Request";
            if (string.Equals(entity, "Deliveries", StringComparison.OrdinalIgnoreCase)) entity = "Delivery";
            if (string.Equals(entity, "Items", StringComparison.OrdinalIgnoreCase)) entity = "Item";
            if (string.Equals(entity, "Suppliers", StringComparison.OrdinalIgnoreCase)) entity = "Supplier";
            if (string.Equals(entity, "Offices", StringComparison.OrdinalIgnoreCase)) entity = "Office";
            if (string.Equals(entity, "Categories", StringComparison.OrdinalIgnoreCase)) entity = "Category";
            if (string.Equals(entity, "Auth", StringComparison.OrdinalIgnoreCase)) entity = "Auth";

            return (action, entity);
        }

        private static string? BuildDetails(PwCStationeryAPI.Models.AuditLog a, string action, string entity, bool showUrl)
        {
            // Prefer a tiny diff if you have change payloads
            var diff = DiffBeforeAfter(a.BeforeJson, a.AfterJson);
            if (!string.IsNullOrEmpty(diff))
            {
                var idPart = string.IsNullOrWhiteSpace(a.EntityId) ? "" : $" #{a.EntityId}";
                return $"{entity}{idPart}: {diff}";
            }

            // Verb-based summary
            if (action is "Approved" or "Rejected" or "Issued" or "Returned" or "Created" or "Updated" or "Deleted" or "Adjusted")
            {
                var idPart = string.IsNullOrWhiteSpace(a.EntityId) ? "" : $" #{a.EntityId}";
                return $"{action} {entity}{idPart}" + (showUrl ? UrlSuffix(a) : "");
            }

            // Reads (if included or 'Viewed')
            if (a.Method?.ToUpperInvariant() == "GET")
            {
                var (page, pageSize, id) = ExtractCommonQuery(a.QueryString);
                if (!string.IsNullOrWhiteSpace(id)) return $"Viewed {entity} #{id}";
                if (a.Action is "List" or "GetAll")
                {
                    var pp = page.HasValue ? $"page {page}" : null;
                    var ps = pageSize.HasValue ? $"size {pageSize}" : null;
                    var suffix = string.Join(", ", new[] { pp, ps }.Where(x => !string.IsNullOrEmpty(x)));
                    return $"Viewed {entity} list" + (suffix == "" ? "" : $" ({suffix})");
                }
            }

            // Fallback (debug)
            return showUrl ? UrlSuffix(a) : null;
        }

        private static string UrlSuffix(PwCStationeryAPI.Models.AuditLog a)
        {
            var url = a.Path + (string.IsNullOrWhiteSpace(a.QueryString) ? "" : a.QueryString);
            return $" · {url} · Status {a.StatusCode} · {a.DurationMs}ms";
        }

        private static (int? page, int? pageSize, string? id) ExtractCommonQuery(string? query)
        {
            if (string.IsNullOrWhiteSpace(query)) return (null, null, null);
            var q = HttpUtility.ParseQueryString(query.TrimStart('?'));
            q.Remove("signal"); q.Remove("skip"); q.Remove("take");
            int? page = null, pageSize = null;
            if (int.TryParse(q.Get("page"), out var p)) page = p;
            if (int.TryParse(q.Get("pageSize"), out var s)) pageSize = s;
            var id = q.Get("id") ?? q.Get("itemId") ?? q.Get("requestId") ?? q.Get("deliveryId") ?? q.Get("stockLevelId");
            return (page, pageSize, id);
        }

        // Tiny, human-friendly diff of top-level JSON fields (max 4 changes)
        private static string? DiffBeforeAfter(string? beforeJson, string? afterJson)
        {
            if (string.IsNullOrWhiteSpace(beforeJson) && string.IsNullOrWhiteSpace(afterJson))
                return null;

            try
            {
                using var docBefore = string.IsNullOrWhiteSpace(beforeJson) ? null : JsonDocument.Parse(beforeJson);
                using var docAfter = string.IsNullOrWhiteSpace(afterJson) ? null : JsonDocument.Parse(afterJson);

                var before = docBefore?.RootElement.ValueKind == JsonValueKind.Object
                    ? docBefore!.RootElement.EnumerateObject().ToDictionary(p => p.Name, p => p.Value, StringComparer.OrdinalIgnoreCase)
                    : new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);

                var after = docAfter?.RootElement.ValueKind == JsonValueKind.Object
                    ? docAfter!.RootElement.EnumerateObject().ToDictionary(p => p.Name, p => p.Value, StringComparer.OrdinalIgnoreCase)
                    : new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);

                var priority = new[] { "status", "quantity", "qty", "stock", "stockLevel", "name", "title", "itemId", "requestId", "deliveryId", "officeId", "categoryId" };
                var keys = new HashSet<string>(before.Keys.Concat(after.Keys), StringComparer.OrdinalIgnoreCase);

                var ordered = priority.Where(keys.Contains)
                                      .Concat(keys.Except(priority, StringComparer.OrdinalIgnoreCase))
                                      .Take(12);

                var changes = new List<string>();
                foreach (var k in ordered)
                {
                    before.TryGetValue(k, out var b);
                    after.TryGetValue(k, out var a);

                    var bs = JsonToShortString(b);
                    var as_ = JsonToShortString(a);

                    if (!string.Equals(bs ?? "", as_ ?? "", StringComparison.Ordinal))
                    {
                        changes.Add($"{k}: {bs}→{as_}");
                        if (changes.Count >= 4) break;
                    }
                }

                return changes.Count == 0 ? null : string.Join("; ", changes);
            }
            catch
            {
                return "changed";
            }
        }

        private static string JsonToShortString(JsonElement value)
        {
            if (value.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null) return "null";

            string s = value.ValueKind switch
            {
                JsonValueKind.String => value.GetString() ?? "",
                JsonValueKind.Number => value.ToString(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                JsonValueKind.Object or JsonValueKind.Array => JsonSerializer.Serialize(value),
                _ => value.ToString()
            };

            s = s.Replace("\r", " ").Replace("\n", " ").Trim();
            if (s.Length > 40) s = s.Substring(0, 37) + "…";
            return s == "" ? "\"\"" : s;
        }
    }
}

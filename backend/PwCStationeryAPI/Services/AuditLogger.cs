using System.Text.Json;
using Microsoft.AspNetCore.Http;
using PwCStationeryAPI.Data;

namespace PwCStationeryAPI.Services
{
    public class AuditLogger
    {
        private readonly ApplicationDbContext _db;
        private readonly IHttpContextAccessor _http;

        private static readonly JsonSerializerOptions _json = new JsonSerializerOptions
        {
            WriteIndented = false
        };

        public AuditLogger(ApplicationDbContext db, IHttpContextAccessor http)
        {
            _db = db; _http = http;
        }

        public async Task LogAsync(string action, string entity, object? entityId, object? beforeObj, object? afterObj)
        {
            var ctx = _http.HttpContext;
            var user = ctx?.User?.Identity?.Name;
            var ip = ctx?.Connection?.RemoteIpAddress?.ToString();

            _db.AuditLogs.Add(new Models.AuditLog
            {
                Action = action,
                Entity = entity,
                EntityId = entityId?.ToString(),
                WhenUtc = DateTime.UtcNow,
                UserName = user,
                ClientIp = ip,
                BeforeJson = beforeObj == null ? null : JsonSerializer.Serialize(beforeObj, _json),
                AfterJson = afterObj == null ? null : JsonSerializer.Serialize(afterObj, _json),
            });
            await _db.SaveChangesAsync();
        }
    }
}

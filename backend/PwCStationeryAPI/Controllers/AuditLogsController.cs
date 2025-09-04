// backend/PwCStationeryAPI/Controllers/AuditLogsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PwCStationeryAPI.Data;     // ApplicationDbContext
using PwCStationeryAPI.Models;

namespace PwCStationeryAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin")] // read-only to SuperAdmin
    public class AuditLogsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public AuditLogsController(ApplicationDbContext db) => _db = db;

        // GET /api/AuditLogs?user=...&path=...&method=GET&from=2025-08-01&to=2025-08-31&page=1&pageSize=50
        [HttpGet]
        public async Task<IActionResult> List([FromQuery] string? user, [FromQuery] string? path,
            [FromQuery] string? method, [FromQuery] DateTime? from, [FromQuery] DateTime? to,
            [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            page = Math.Max(1, page);
            pageSize = Math.Clamp(pageSize, 1, 200);

            var q = _db.AuditLogs.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(user))
                q = q.Where(x => (x.UserName ?? "").Contains(user) || (x.UserId ?? "").Contains(user));

            if (!string.IsNullOrWhiteSpace(path))
                q = q.Where(x => (x.Path ?? "").Contains(path));

            if (!string.IsNullOrWhiteSpace(method))
                q = q.Where(x => x.Method == method);

            if (from.HasValue) q = q.Where(x => x.TimestampUtc >= from.Value);
            if (to.HasValue) q = q.Where(x => x.TimestampUtc <= to.Value);

            var total = await q.CountAsync();
            var data = await q.OrderByDescending(x => x.TimestampUtc)
                              .Skip((page - 1) * pageSize)
                              .Take(pageSize)
                              .ToListAsync();

            return Ok(new { total, page, pageSize, data });
        }
    }
}

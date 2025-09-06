using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PwCStationeryAPI.Data;
using PwCStationeryAPI.Dtos.Items;
using PwCStationeryAPI.Services;

namespace PwCStationeryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IssuesController : ControllerBase
    {
        private readonly IStockMutationService _svc;

        public IssuesController(IStockMutationService svc) => _svc = svc;

        [HttpPost]
        [Authorize(Roles = "Manager,Admin,SuperAdmin")]
        public async Task<IActionResult> Create([FromBody] CreateIssueDto dto)
        {
            var actorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(actorId))
                return Unauthorized("No user id claim.");

            try
            {
                var issue = await _svc.CreateIssueAsync(dto, actorId);
                return Ok(issue);
            }
            catch (InvalidOperationException ex)
            {
                var msg = ex.Message;

                if (msg.Contains("already issued", StringComparison.OrdinalIgnoreCase))
                {
                    var db = HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
                    var existing = await db.Issues
                        .Include(i => i.Lines)
                        .FirstOrDefaultAsync(i => i.RequestId == dto.RequestId);

                    return existing is null ? Conflict(msg) : Ok(existing);
                }

                if (msg.Contains("not found", StringComparison.OrdinalIgnoreCase)) return NotFound(msg);
                if (msg.Contains("not approved", StringComparison.OrdinalIgnoreCase)) return BadRequest(msg);
                if (msg.Contains("Insufficient stock", StringComparison.OrdinalIgnoreCase) ||
                    msg.Contains("Quantity must be", StringComparison.OrdinalIgnoreCase)) return BadRequest(msg);

                return BadRequest(msg); // <-- default, NOT Problem(...)
            }
            catch (DbUpdateException)
            {
                return Conflict("Database constraint violation (possible duplicate issue for this request).");
            }
            catch (Exception ex)
            {
                return Problem(title: ex.Message, statusCode: 500); // real unknown errors only
            }

        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Manager,Admin,SuperAdmin")]
        public async Task<IActionResult> GetById(int id, [FromServices] ApplicationDbContext db)
        {
            var issue = await db.Issues
                .Include(i => i.Lines)
                .ThenInclude(l => l.Item)
                .FirstOrDefaultAsync(i => i.Id == id);

            return issue is null ? NotFound() : Ok(issue);
        }

        [HttpGet("by-request/{requestId:int}")]
        [Authorize(Roles = "Manager,Admin,SuperAdmin")]
        public async Task<IActionResult> GetByRequest(int requestId, [FromServices] ApplicationDbContext db)
        {
            var issue = await db.Issues
                .Include(i => i.Lines)
                .ThenInclude(l => l.Item)
                .FirstOrDefaultAsync(i => i.RequestId == requestId);

            return issue is null ? NotFound() : Ok(issue);
        }
    }
}

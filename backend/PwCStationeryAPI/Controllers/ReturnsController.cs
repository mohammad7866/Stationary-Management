using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
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
    public class ReturnsController : ControllerBase
    {
        private readonly IStockMutationService _svc;

        public ReturnsController(IStockMutationService svc) => _svc = svc;

        [HttpPost]
        [Authorize(Roles = "Manager,Admin,SuperAdmin")]
        public async Task<IActionResult> Create([FromBody] CreateReturnDto dto)
        {
            var actorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(actorId))
                return Unauthorized("No user id claim.");

            try
            {
                var ret = await _svc.CreateReturnAsync(dto, actorId);
                return Ok(ret);
            }
            catch (InvalidOperationException ex)
            {
                var msg = ex.Message;

                if (msg.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return NotFound(msg);

                if (msg.Contains("exceeds issued", StringComparison.OrdinalIgnoreCase) ||
                    msg.Contains("Quantity must be", StringComparison.OrdinalIgnoreCase) ||
                    msg.Contains("Insufficient stock", StringComparison.OrdinalIgnoreCase))
                    return BadRequest(msg);

                return BadRequest(msg);
            }
            catch (DbUpdateException)
            {
                return Conflict("Database constraint violation.");
            }
            catch (Exception ex)
            {
                return Problem(title: ex.Message, statusCode: 500);
            }
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Manager,Admin,SuperAdmin")]
        public async Task<IActionResult> GetById(int id, [FromServices] ApplicationDbContext db)
        {
            var ret = await db.Returns
                .Include(r => r.Lines).ThenInclude(l => l.Item)
                .Include(r => r.Issue)
                .FirstOrDefaultAsync(r => r.Id == id);

            return ret is null ? NotFound() : Ok(ret);
        }

        [HttpGet("by-issue/{issueId:int}")]
        [Authorize(Roles = "Manager,Admin,SuperAdmin")]
        public async Task<IActionResult> GetByIssue(int issueId, [FromServices] ApplicationDbContext db)
        {
            var list = await db.Returns
                .Where(r => r.IssueId == issueId)
                .Include(r => r.Lines).ThenInclude(l => l.Item)
                .OrderBy(r => r.ReturnedAt)
                .ToListAsync();

            return Ok(list);
        }
    }
}

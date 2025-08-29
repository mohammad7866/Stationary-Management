// backend/PwCStationeryAPI/Controllers/RequestsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PwCStationeryAPI.Data;
using PwCStationeryAPI.Dtos.Requests;
using PwCStationeryAPI.Models;
using PwCStationeryAPI.Services;

namespace PwCStationeryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RequestsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly AuditLogger _audit;
        public RequestsController(ApplicationDbContext db, AuditLogger audit)
        {
            _db = db; _audit = audit;
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReadRequestDto>>> GetAll(
            [FromQuery] string? status = null,
            [FromQuery] string? office = null,
            [FromQuery] string? itemName = null)
        {
            var q = _db.Requests.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(status)) q = q.Where(r => r.Status == status);
            if (!string.IsNullOrWhiteSpace(office)) q = q.Where(r => r.Office == office);
            if (!string.IsNullOrWhiteSpace(itemName)) q = q.Where(r => r.ItemName == itemName);

            var list = await q
                .OrderByDescending(r => r.CreatedAtUtc)
                .Select(r => new ReadRequestDto
                {
                    Id = r.Id,
                    ItemName = r.ItemName,
                    Office = r.Office,
                    Quantity = r.Quantity,
                    Purpose = r.Purpose,
                    Status = r.Status,
                    CreatedAt = r.CreatedAtUtc // your DTO uses CreatedAt (nullable) → map from CreatedAtUtc
                })
                .ToListAsync();

            return Ok(list);
        }

        [Authorize]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ReadRequestDto>> GetOne(int id)
        {
            var dto = await _db.Requests.AsNoTracking()
                .Where(r => r.Id == id)
                .Select(r => new ReadRequestDto
                {
                    Id = r.Id,
                    ItemName = r.ItemName,
                    Office = r.Office,
                    Quantity = r.Quantity,
                    Purpose = r.Purpose,
                    Status = r.Status,
                    CreatedAt = r.CreatedAtUtc
                })
                .FirstOrDefaultAsync();

            return dto is null ? NotFound() : Ok(dto);
        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRequestDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var entity = new Request
            {
                ItemName = dto.ItemName,
                Office = dto.Office,
                Quantity = dto.Quantity,
                Purpose = dto.Purpose,
                Status = string.IsNullOrWhiteSpace(dto.Status) ? "Pending" : dto.Status,
                CreatedAtUtc = DateTime.UtcNow,
                DecisionAtUtc = null
            };

            _db.Requests.Add(entity);
            await _db.SaveChangesAsync();
            await _audit.LogAsync("Request.Create", "Request", entity.Id, null, entity);

            var read = new ReadRequestDto
            {
                Id = entity.Id,
                ItemName = entity.ItemName,
                Office = entity.Office,
                Quantity = entity.Quantity,
                Purpose = entity.Purpose,
                Status = entity.Status,
                CreatedAt = entity.CreatedAtUtc
            };

            return CreatedAtAction(nameof(GetOne), new { id = read.Id }, read);
        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateRequestDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var entity = await _db.Requests.FirstOrDefaultAsync(r => r.Id == id);
            if (entity is null) return NotFound();

            var before = new
            {
                entity.Id,
                entity.Quantity,
                entity.Purpose,
                entity.Status,
                entity.DecisionAtUtc
            };

            entity.Quantity = dto.Quantity;
            entity.Purpose = dto.Purpose;
            entity.Status = dto.Status;

            // if status changed to Approved/Rejected without using status endpoints, set decision time
            if ((dto.Status == "Approved" || dto.Status == "Rejected") && entity.DecisionAtUtc is null)
                entity.DecisionAtUtc = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            await _audit.LogAsync("Request.Update", "Request", id, before, entity);
            return NoContent();
        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpPost("{id:int}/status")]
        public async Task<IActionResult> SetStatus(int id, [FromBody] UpdateRequestStatusDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var entity = await _db.Requests.FirstOrDefaultAsync(r => r.Id == id);
            if (entity is null) return NotFound();

            var before = new { entity.Id, entity.Status, entity.DecisionAtUtc };

            entity.Status = dto.Status;
            if (dto.Status == "Approved" || dto.Status == "Rejected")
                entity.DecisionAtUtc = DateTime.UtcNow;
            else if (dto.Status == "Pending")
                entity.DecisionAtUtc = null;

            await _db.SaveChangesAsync();
            await _audit.LogAsync("Request.Status", "Request", id, before, new { entity.Status, entity.DecisionAtUtc });

            return Ok(new { id = entity.Id, status = entity.Status, decisionAtUtc = entity.DecisionAtUtc });
        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpPost("{id:int}/approve")]
        public Task<IActionResult> Approve(int id)
            => SetStatus(id, new UpdateRequestStatusDto { Status = "Approved" });

        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpPost("{id:int}/reject")]
        public Task<IActionResult> Reject(int id)
            => SetStatus(id, new UpdateRequestStatusDto { Status = "Rejected" });

        [Authorize(Roles = "SuperAdmin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _db.Requests.FindAsync(id);
            if (entity is null) return NotFound();

            var before = new
            {
                entity.Id,
                entity.ItemName,
                entity.Office,
                entity.Quantity,
                entity.Purpose,
                entity.Status,
                entity.CreatedAtUtc,
                entity.DecisionAtUtc
            };

            _db.Requests.Remove(entity);
            await _db.SaveChangesAsync();
            await _audit.LogAsync("Request.Delete", "Request", id, before, null);

            return NoContent();
        }
    }
}

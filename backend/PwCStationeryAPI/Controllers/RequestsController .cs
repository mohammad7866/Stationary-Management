// backend/PwCStationeryAPI/Controllers/RequestsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PwCStationeryAPI.Data;
using PwCStationeryAPI.Models;

namespace PwCStationeryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RequestsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public RequestsController(ApplicationDbContext db) => _db = db;

        /// <summary>
        /// Get requests (filter by status/office optional)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? status = null, [FromQuery] string? office = null)
        {
            var q = _db.Requests.AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
                q = q.Where(r => r.Status == status);

            if (!string.IsNullOrWhiteSpace(office))
                q = q.Where(r => r.Office == office);

            var data = await q
                .OrderByDescending(r => r.CreatedAtUtc)
                .ToListAsync();

            return Ok(data);
        }

        /// <summary>
        /// Get a single request by id
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetOne(int id)
        {
            var req = await _db.Requests.FindAsync(id);
            return req is null ? NotFound() : Ok(req);
        }

        /// <summary>
        /// Create a new request (defaults to Pending)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create(Request model)
        {
            model.Status = "Pending";
            model.CreatedAtUtc = DateTime.UtcNow;

            _db.Requests.Add(model);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOne), new { id = model.Id }, model);
        }

        /// <summary>
        /// Update a request (only for correcting details while Pending)
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, Request model)
        {
            if (id != model.Id) return BadRequest("Mismatched id.");
            var existing = await _db.Requests.FindAsync(id);
            if (existing is null) return NotFound();

            if (existing.Status != "Pending")
                return BadRequest("Only pending requests can be edited.");

            // Update allowed fields
            existing.ItemName = model.ItemName;
            existing.Quantity = model.Quantity;
            existing.Office = model.Office;
            existing.Purpose = model.Purpose;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Approve a pending request
        /// </summary>
        [HttpPost("{id:int}/approve")]
        public async Task<IActionResult> Approve(int id)
        {
            var req = await _db.Requests.FindAsync(id);
            if (req is null) return NotFound();
            if (req.Status != "Pending") return BadRequest("Request already decided.");

            req.Status = "Approved";
            req.DecisionAtUtc = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return Ok(req);
        }

        /// <summary>
        /// Reject a pending request
        /// </summary>
        [HttpPost("{id:int}/reject")]
        public async Task<IActionResult> Reject(int id)
        {
            var req = await _db.Requests.FindAsync(id);
            if (req is null) return NotFound();
            if (req.Status != "Pending") return BadRequest("Request already decided.");

            req.Status = "Rejected";
            req.DecisionAtUtc = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return Ok(req);
        }

        /// <summary>
        /// Delete a request (allowed anytime)
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var req = await _db.Requests.FindAsync(id);
            if (req is null) return NotFound();

            _db.Requests.Remove(req);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}

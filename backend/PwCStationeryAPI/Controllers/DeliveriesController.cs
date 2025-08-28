// backend/PwCStationeryAPI/Controllers/DeliveriesController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PwCStationeryAPI.Data;
using PwCStationeryAPI.Models;

namespace PwCStationeryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeliveriesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public DeliveriesController(ApplicationDbContext db) => _db = db;

        /// <summary>
        /// Get deliveries with optional filters: supplierId, office, status, from (yyyy-MM-dd), to (yyyy-MM-dd)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int? supplierId = null,
            [FromQuery] string? office = null,
            [FromQuery] string? status = null,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null)
        {
            var q = _db.Deliveries
                .Include(d => d.Supplier)
                .AsQueryable();

            if (supplierId.HasValue) q = q.Where(d => d.SupplierId == supplierId.Value);
            if (!string.IsNullOrWhiteSpace(office)) q = q.Where(d => d.Office == office);
            if (!string.IsNullOrWhiteSpace(status)) q = q.Where(d => d.Status == status);
            if (from.HasValue) q = q.Where(d => d.ScheduledDateUtc >= from.Value);
            if (to.HasValue) q = q.Where(d => d.ScheduledDateUtc <= to.Value);

            var data = await q
                .OrderByDescending(d => d.ScheduledDateUtc)
                .ToListAsync();

            return Ok(data);
        }

        /// <summary>
        /// Get one delivery
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetOne(int id)
        {
            var d = await _db.Deliveries
                .Include(x => x.Supplier)
                .FirstOrDefaultAsync(x => x.Id == id);

            return d is null ? NotFound() : Ok(d);
        }

        /// <summary>
        /// Create a delivery (Status defaults to Pending if not provided)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create(Delivery model)
        {
            if (string.IsNullOrWhiteSpace(model.Status))
                model.Status = "Pending";

            // Normalize dates: store as UTC (assuming input without timezone is local)
            model.ScheduledDateUtc = DateTime.SpecifyKind(model.ScheduledDateUtc, DateTimeKind.Utc);
            if (model.ArrivalDateUtc.HasValue)
                model.ArrivalDateUtc = DateTime.SpecifyKind(model.ArrivalDateUtc.Value, DateTimeKind.Utc);

            // If arrival provided on creation, set status if not set
            if (model.ArrivalDateUtc.HasValue && string.IsNullOrWhiteSpace(model.Status))
                model.Status = "On Time";

            _db.Deliveries.Add(model);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOne), new { id = model.Id }, model);
        }

        /// <summary>
        /// Update a delivery (use to edit product/office/dates/supplier/status)
        /// </summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, Delivery model)
        {
            if (id != model.Id) return BadRequest("Mismatched id.");

            var existing = await _db.Deliveries.FindAsync(id);
            if (existing is null) return NotFound();

            existing.Product = model.Product;
            existing.SupplierId = model.SupplierId;
            existing.Office = model.Office;
            existing.ScheduledDateUtc = DateTime.SpecifyKind(model.ScheduledDateUtc, DateTimeKind.Utc);
            existing.ArrivalDateUtc = model.ArrivalDateUtc.HasValue
                ? DateTime.SpecifyKind(model.ArrivalDateUtc.Value, DateTimeKind.Utc)
                : null;
            existing.Status = model.Status;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Mark a delivery as arrived now; status auto-calculated (On Time/Delayed)
        /// </summary>
        [HttpPost("{id:int}/arrive")]
        public async Task<IActionResult> MarkArrived(int id)
        {
            var d = await _db.Deliveries.FindAsync(id);
            if (d is null) return NotFound();

            d.ArrivalDateUtc = DateTime.UtcNow;
            d.Status = (d.ArrivalDateUtc.Value.Date <= d.ScheduledDateUtc.Date) ? "On Time" : "Delayed";

            await _db.SaveChangesAsync();
            return Ok(d);
        }

        /// <summary>
        /// Delete a delivery
        /// </summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var d = await _db.Deliveries.FindAsync(id);
            if (d is null) return NotFound();

            _db.Deliveries.Remove(d);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Supplier performance: average arrival delay days (negative/zero = on-time/early)
        /// </summary>
        [HttpGet("metrics/supplier/{supplierId:int}")]
        public async Task<IActionResult> SupplierMetrics(int supplierId, [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
        {
            var q = _db.Deliveries
                .Where(d => d.SupplierId == supplierId && d.ArrivalDateUtc != null);

            if (from.HasValue) q = q.Where(d => d.ScheduledDateUtc >= from.Value);
            if (to.HasValue) q = q.Where(d => d.ScheduledDateUtc <= to.Value);

            var list = await q.ToListAsync();
            if (list.Count == 0)
                return Ok(new { supplierId, count = 0, avgDelayDays = (double?)null });

            var avg = list.Average(d => (d.ArrivalDateUtc!.Value.Date - d.ScheduledDateUtc.Date).TotalDays);
            return Ok(new { supplierId, count = list.Count, avgDelayDays = Math.Round(avg, 2) });
        }
    }
}

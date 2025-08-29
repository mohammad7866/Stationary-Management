// backend/PwCStationeryAPI/Controllers/StockLevelsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PwCStationeryAPI.Data;
using PwCStationeryAPI.Dtos.StockLevels;
using PwCStationeryAPI.Models;
using PwCStationeryAPI.Services;
using Microsoft.AspNetCore.Authorization;

namespace PwCStationERYAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StockLevelsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly AuditLogger _audit;

        public StockLevelsController(ApplicationDbContext db, AuditLogger audit)
        {
            _db = db;
            _audit = audit;
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReadStockLevelDto>>> GetAll(
            [FromQuery] int? officeId = null,
            [FromQuery] int? itemId = null)
        {
            var q = _db.StockLevels
                .Include(s => s.Item)
                .Include(s => s.Office)
                .AsNoTracking()
                .AsQueryable();

            if (officeId.HasValue) q = q.Where(s => s.OfficeId == officeId.Value);
            if (itemId.HasValue) q = q.Where(s => s.ItemId == itemId.Value);

            var list = await q
                .OrderBy(s => s.Item.Name).ThenBy(s => s.Office.Name)
                .Select(s => new ReadStockLevelDto
                {
                    Id = s.Id,
                    ItemId = s.ItemId,
                    ItemName = s.Item.Name,
                    OfficeId = s.OfficeId,
                    OfficeName = s.Office.Name,
                    Quantity = s.Quantity,
                    ReorderThreshold = s.ReorderThreshold,
                    IsLow = s.Quantity <= s.ReorderThreshold
                })
                .ToListAsync();

            return Ok(list);
        }

        [Authorize]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ReadStockLevelDto>> GetOne(int id)
        {
            var s = await _db.StockLevels
                .Include(x => x.Item)
                .Include(x => x.Office)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (s is null) return NotFound();

            return Ok(new ReadStockLevelDto
            {
                Id = s.Id,
                ItemId = s.ItemId,
                ItemName = s.Item.Name,
                OfficeId = s.OfficeId,
                OfficeName = s.Office.Name,
                Quantity = s.Quantity,
                ReorderThreshold = s.ReorderThreshold,
                IsLow = s.Quantity <= s.ReorderThreshold
            });
        }

        // POST: api/stocklevels
        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpPost]
        public async Task<IActionResult> Create(StockLevel model)
        {
            var exists = await _db.StockLevels
                .AnyAsync(s => s.ItemId == model.ItemId && s.OfficeId == model.OfficeId);
            if (exists) return Conflict("StockLevel for this (Item, Office) already exists.");

            _db.StockLevels.Add(model);
            await _db.SaveChangesAsync();

            await _audit.LogAsync("StockLevel.Create", "StockLevel", model.Id, null, model);
            return CreatedAtAction(nameof(GetOne), new { id = model.Id }, model);
        }

        // PUT: api/stocklevels/5
        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, StockLevel model)
        {
            if (id != model.Id) return BadRequest("Mismatched id.");

            var existing = await _db.StockLevels.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
            if (existing is null) return NotFound();

            // Only update simple fields; ItemId/OfficeId could be allowed if you want to move the record
            var before = new
            {
                existing.Id,
                existing.ItemId,
                existing.OfficeId,
                existing.Quantity,
                existing.ReorderThreshold
            };

            var entity = await _db.StockLevels.FirstAsync(s => s.Id == id);
            entity.Quantity = model.Quantity;
            entity.ReorderThreshold = model.ReorderThreshold;
            // (Optional) allow changing ItemId/OfficeId:
            // entity.ItemId = model.ItemId;
            // entity.OfficeId = model.OfficeId;

            await _db.SaveChangesAsync();
            await _audit.LogAsync("StockLevel.Update", "StockLevel", id, before, entity);
            return NoContent();
        }

        // POST: api/stocklevels/5/adjust
        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpPost("{id:int}/adjust")]
        public async Task<IActionResult> Adjust(int id, [FromBody] int delta)
        {
            var s = await _db.StockLevels.FindAsync(id);
            if (s is null) return NotFound();

            var before = new { s.Id, s.Quantity };
            s.Quantity += delta; // clamp if desired: s.Quantity = Math.Max(0, s.Quantity + delta);
            await _db.SaveChangesAsync();

            await _audit.LogAsync("StockLevel.Adjust", "StockLevel", id, before, new { s.Id, s.Quantity });
            return Ok(new
            {
                s.Id,
                s.ItemId,
                s.OfficeId,
                s.Quantity,
                s.ReorderThreshold,
                IsLow = s.Quantity <= s.ReorderThreshold
            });
        }

        // DELETE: api/stocklevels/5
        [Authorize(Roles = "SuperAdmin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var s = await _db.StockLevels.FindAsync(id);
            if (s is null) return NotFound();

            var before = new
            {
                s.Id,
                s.ItemId,
                s.OfficeId,
                s.Quantity,
                s.ReorderThreshold
            };

            _db.StockLevels.Remove(s);
            await _db.SaveChangesAsync();

            await _audit.LogAsync("StockLevel.Delete", "StockLevel", id, before, null);
            return NoContent();
        }
    }
}

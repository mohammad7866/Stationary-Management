using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PwCStationeryAPI.Data;
using PwCStationeryAPI.Models;

namespace PwCStationeryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StockLevelsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public StockLevelsController(ApplicationDbContext db) => _db = db;

        // GET: api/stocklevels?officeId=1&itemId=2
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? officeId = null, [FromQuery] int? itemId = null)
        {
            var q = _db.StockLevels
                .Include(s => s.Item)
                .Include(s => s.Office)
                .AsQueryable();

            if (officeId.HasValue) q = q.Where(s => s.OfficeId == officeId.Value);
            if (itemId.HasValue) q = q.Where(s => s.ItemId == itemId.Value);

            return Ok(await q.OrderBy(s => s.Item.Name).ThenBy(s => s.Office.Name).ToListAsync());
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetOne(int id)
        {
            var s = await _db.StockLevels
                .Include(x => x.Item)
                .Include(x => x.Office)
                .FirstOrDefaultAsync(x => x.Id == id);

            return s is null ? NotFound() : Ok(s);
        }

        [HttpPost]
        public async Task<IActionResult> Create(StockLevel model)
        {
            // Enforce uniqueness per (Item, Office)
            var exists = await _db.StockLevels.AnyAsync(s => s.ItemId == model.ItemId && s.OfficeId == model.OfficeId);
            if (exists) return Conflict("StockLevel for this (Item, Office) already exists.");

            _db.StockLevels.Add(model);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetOne), new { id = model.Id }, model);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, StockLevel model)
        {
            if (id != model.Id) return BadRequest("Mismatched id.");
            if (!await _db.StockLevels.AnyAsync(s => s.Id == id)) return NotFound();

            _db.Entry(model).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // PATCH-like: adjust quantity
        [HttpPost("{id:int}/adjust")]
        public async Task<IActionResult> Adjust(int id, [FromBody] int delta)
        {
            var s = await _db.StockLevels.FindAsync(id);
            if (s is null) return NotFound();

            s.Quantity += delta;
            await _db.SaveChangesAsync();
            return Ok(s);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var s = await _db.StockLevels.FindAsync(id);
            if (s is null) return NotFound();

            _db.StockLevels.Remove(s);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}

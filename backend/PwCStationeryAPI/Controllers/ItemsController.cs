// backend/PwCStationeryAPI/Controllers/ItemsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PwCStationeryAPI.Data;
using PwCStationeryAPI.Models;

namespace PwCStationeryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ItemsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public ItemsController(ApplicationDbContext db) => _db = db;

        // GET: api/items
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? categoryType = null)
        {
            var q = _db.Items
                .Include(i => i.Category)
                .Include(i => i.Supplier)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(categoryType))
                q = q.Where(i => i.Category.Name == categoryType); // ✅ filter on Category.Name

            return Ok(await q.OrderBy(i => i.Name).ToListAsync());
        }


        // GET: api/items/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetOne(int id)
        {
            var item = await _db.Items
                .Include(i => i.Category)
                .Include(i => i.Supplier)
                .FirstOrDefaultAsync(i => i.Id == id);

            return item is null ? NotFound() : Ok(item);
        }

        // POST: api/items
        [HttpPost]
        public async Task<IActionResult> Create(Item item)
        {
            _db.Items.Add(item);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetOne), new { id = item.Id }, item);
        }

        // PUT: api/items/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, Item update)
        {
            if (id != update.Id) return BadRequest("Mismatched id.");

            var exists = await _db.Items.AnyAsync(i => i.Id == id);
            if (!exists) return NotFound();

            _db.Entry(update).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/items/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _db.Items.FindAsync(id);
            if (item is null) return NotFound();

            _db.Items.Remove(item);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}

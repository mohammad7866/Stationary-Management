using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PwCStationeryAPI.Data;
using PwCStationeryAPI.Models;

namespace PwCStationeryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SuppliersController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public SuppliersController(ApplicationDbContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? q = null)
        {
            var query = _db.Suppliers.AsQueryable();
            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(s => s.Name.Contains(q));
            return Ok(await query.OrderBy(s => s.Name).ToListAsync());
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetOne(int id)
        {
            var s = await _db.Suppliers.FindAsync(id);
            return s is null ? NotFound() : Ok(s);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Supplier model)
        {
            _db.Suppliers.Add(model);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetOne), new { id = model.Id }, model);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, Supplier model)
        {
            if (id != model.Id) return BadRequest("Mismatched id.");
            if (!await _db.Suppliers.AnyAsync(x => x.Id == id)) return NotFound();

            _db.Entry(model).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var s = await _db.Suppliers.FindAsync(id);
            if (s is null) return NotFound();

            _db.Suppliers.Remove(s);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}

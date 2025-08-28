using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PwCStationeryAPI.Data;
using PwCStationeryAPI.Models;

namespace PwCStationeryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public CategoriesController(ApplicationDbContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _db.Categories.OrderBy(c => c.Name).ToListAsync());

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetOne(int id)
        {
            var c = await _db.Categories.FindAsync(id);
            return c is null ? NotFound() : Ok(c);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Category model)
        {
            _db.Categories.Add(model);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetOne), new { id = model.Id }, model);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, Category model)
        {
            if (id != model.Id) return BadRequest("Mismatched id.");
            if (!await _db.Categories.AnyAsync(x => x.Id == id)) return NotFound();

            _db.Entry(model).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var c = await _db.Categories.FindAsync(id);
            if (c is null) return NotFound();

            _db.Categories.Remove(c);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}

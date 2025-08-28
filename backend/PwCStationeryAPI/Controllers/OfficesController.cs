using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PwCStationeryAPI.Data;
using PwCStationeryAPI.Models;

namespace PwCStationeryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OfficesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public OfficesController(ApplicationDbContext db) => _db = db;

        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _db.Offices.OrderBy(o => o.Name).ToListAsync());

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetOne(int id)
        {
            var o = await _db.Offices.FindAsync(id);
            return o is null ? NotFound() : Ok(o);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Office model)
        {
            _db.Offices.Add(model);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetOne), new { id = model.Id }, model);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, Office model)
        {
            if (id != model.Id) return BadRequest("Mismatched id.");
            if (!await _db.Offices.AnyAsync(x => x.Id == id)) return NotFound();

            _db.Entry(model).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var o = await _db.Offices.FindAsync(id);
            if (o is null) return NotFound();

            _db.Offices.Remove(o);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}

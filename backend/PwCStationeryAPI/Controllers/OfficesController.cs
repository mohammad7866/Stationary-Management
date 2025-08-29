using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PwCStationeryAPI.Data;
using PwCStationeryAPI.Models;
using Microsoft.AspNetCore.Authorization;

namespace PwCStationeryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OfficesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public OfficesController(ApplicationDbContext db) => _db = db;

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _db.Offices.OrderBy(o => o.Name).ToListAsync());

        [Authorize]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetOne(int id)
        {
            var o = await _db.Offices.FindAsync(id);
            return o is null ? NotFound() : Ok(o);
        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpPost]
        public async Task<IActionResult> Create(Office model)
        {
            _db.Offices.Add(model);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(GetOne), new { id = model.Id }, model);
        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, Office model)
        {
            if (id != model.Id) return BadRequest("Mismatched id.");
            if (!await _db.Offices.AnyAsync(x => x.Id == id)) return NotFound();

            _db.Entry(model).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [Authorize(Roles = "SuperAdmin")]
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

// backend/PwCStationeryAPI/Controllers/SuppliersController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PwCStationeryAPI.Data;
using PwCStationeryAPI.Dtos.Suppliers;
using PwCStationeryAPI.Models;
using Microsoft.AspNetCore.Authorization;

namespace PwCStationeryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SuppliersController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public SuppliersController(ApplicationDbContext db) => _db = db;

        /// <summary>
        /// List suppliers with optional search (q) and paging.
        /// </summary>
        /// <param name="q">Search in name, email, phone (case-insensitive)</param>
        /// <param name="page">1-based page number</param>
        /// <param name="pageSize">Items per page (max 100)</param>
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? q = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

            var query = _db.Suppliers.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim().ToLower();
                query = query.Where(s =>
                    EF.Functions.Like(s.Name.ToLower(), $"%{term}%") ||
                    (s.ContactEmail != null && EF.Functions.Like(s.ContactEmail.ToLower(), $"%{term}%")) ||
                    (s.Phone != null && EF.Functions.Like(s.Phone.ToLower(), $"%{term}%")));
            }

            var total = await query.CountAsync();

            var data = await query
                .OrderBy(s => s.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new ReadSupplierDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    ContactEmail = s.ContactEmail,
                    Phone = s.Phone
                })
                .ToListAsync();

            return Ok(new { total, page, pageSize, data });
        }

        [Authorize]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetOne(int id)
        {
            var dto = await _db.Suppliers
                .AsNoTracking()
                .Where(s => s.Id == id)
                .Select(s => new ReadSupplierDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    ContactEmail = s.ContactEmail,
                    Phone = s.Phone
                })
                .FirstOrDefaultAsync();

            return dto is null ? NotFound() : Ok(dto);
        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSupplierDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var entity = new Supplier
            {
                Name = dto.Name,
                ContactEmail = dto.ContactEmail,
                Phone = dto.Phone
            };

            _db.Suppliers.Add(entity);
            await _db.SaveChangesAsync();

            var created = new ReadSupplierDto
            {
                Id = entity.Id,
                Name = entity.Name,
                ContactEmail = entity.ContactEmail,
                Phone = entity.Phone
            };

            return CreatedAtAction(nameof(GetOne), new { id = created.Id }, created);
        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateSupplierDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var entity = await _db.Suppliers.FirstOrDefaultAsync(s => s.Id == id);
            if (entity is null) return NotFound();

            entity.Name = dto.Name;
            entity.ContactEmail = dto.ContactEmail;
            entity.Phone = dto.Phone;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _db.Suppliers.FindAsync(id);
            if (entity is null) return NotFound();

            // Optional: prevent delete if referenced by Deliveries/Items (if desired)
            _db.Suppliers.Remove(entity);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}

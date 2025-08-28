// backend/PwCStationeryAPI/Controllers/ItemsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PwCStationeryAPI.Data;
using PwCStationeryAPI.Models;
using PwCStationeryAPI.Dtos.Items; // <- add this (see note below)

namespace PwCStationeryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ItemsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public ItemsController(ApplicationDbContext db) => _db = db;

        /// <summary>
        /// List items with optional search (q), category filter (by Category.Name), and paging.
        /// </summary>
        /// <param name="q">Search in name/description (case-insensitive)</param>
        /// <param name="categoryType">Exact Category.Name match</param>
        /// <param name="page">1-based page number</param>
        /// <param name="pageSize">Items per page (max 100)</param>
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? q = null,
            [FromQuery] string? categoryType = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

            var query = _db.Items
                .Include(i => i.Category)
                .Include(i => i.Supplier)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(categoryType))
                query = query.Where(i => i.Category.Name == categoryType);

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim().ToLower();
                query = query.Where(i =>
                    EF.Functions.Like(i.Name.ToLower(), $"%{term}%") ||
                    EF.Functions.Like(i.Description.ToLower(), $"%{term}%"));
            }

            var total = await query.CountAsync();

            var data = await query
                .OrderBy(i => i.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new { total, page, pageSize, data });
        }

        /// <summary>Get a single item by id (includes Category and Supplier).</summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetOne(int id)
        {
            var item = await _db.Items
                .Include(i => i.Category)
                .Include(i => i.Supplier)
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == id);

            return item is null ? NotFound() : Ok(item);
        }

        /// <summary>Create a new item.</summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateItemDto dto)
        {
            // [ApiController] auto-validates, but this keeps it explicit:
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            // Optional: light FK existence checks for nicer 400s (instead of FK exceptions)
            var catExists = await _db.Categories.AnyAsync(c => c.Id == dto.CategoryId);
            if (!catExists) return BadRequest($"CategoryId {dto.CategoryId} does not exist.");

            if (dto.SupplierId is int sid)
            {
                var supExists = await _db.Suppliers.AnyAsync(s => s.Id == sid);
                if (!supExists) return BadRequest($"SupplierId {sid} does not exist.");
            }

            var entity = new Item
            {
                Name = dto.Name,
                Description = dto.Description,
                CategoryId = dto.CategoryId,
                SupplierId = dto.SupplierId
            };

            _db.Items.Add(entity);
            await _db.SaveChangesAsync();

            // Return with includes for a nicer Created payload
            var created = await _db.Items
                .Include(i => i.Category)
                .Include(i => i.Supplier)
                .AsNoTracking()
                .FirstAsync(i => i.Id == entity.Id);

            return CreatedAtAction(nameof(GetOne), new { id = created.Id }, created);
        }

        /// <summary>Update an existing item.</summary>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateItemDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var entity = await _db.Items.FirstOrDefaultAsync(i => i.Id == id);
            if (entity is null) return NotFound();

            // Optional FK checks (same as in Create)
            var catExists = await _db.Categories.AnyAsync(c => c.Id == dto.CategoryId);
            if (!catExists) return BadRequest($"CategoryId {dto.CategoryId} does not exist.");

            if (dto.SupplierId is int sid)
            {
                var supExists = await _db.Suppliers.AnyAsync(s => s.Id == sid);
                if (!supExists) return BadRequest($"SupplierId {sid} does not exist.");
            }

            entity.Name = dto.Name;
            entity.Description = dto.Description;
            entity.CategoryId = dto.CategoryId;
            entity.SupplierId = dto.SupplierId;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>Delete an item.</summary>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _db.Items.FindAsync(id);
            if (entity is null) return NotFound();

            _db.Items.Remove(entity);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PwCStationeryAPI.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PwCStationeryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReplenishmentController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public ReplenishmentController(ApplicationDbContext db)
        {
            _db = db;
        }

        public record LowStockSuggestionDto(
            int StockLevelId,
            string OfficeName,
            string ItemName,
            int Quantity,
            int? ReorderThreshold,
            string? SupplierName
        );

        // Only Admin/SuperAdmin can see low-stock suggestions
        [HttpGet("suggestions")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<ActionResult<IEnumerable<LowStockSuggestionDto>>> GetSuggestions(CancellationToken ct)
        {
            var suggestions = await _db.StockLevels
                .AsNoTracking()
                .Include(s => s.Item).ThenInclude(i => i.Supplier)
                .Include(s => s.Office)
                .Where(s => s.ReorderThreshold != null && s.Quantity <= s.ReorderThreshold)
                .Select(s => new LowStockSuggestionDto(
                    s.Id,
                    s.Office.Name,
                    s.Item.Name,
                    s.Quantity,
                    s.ReorderThreshold,
                    s.Item.Supplier != null ? s.Item.Supplier.Name : null
                ))
                .ToListAsync(ct);

            return Ok(suggestions);
        }
    }
}

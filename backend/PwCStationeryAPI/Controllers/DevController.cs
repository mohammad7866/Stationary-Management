// backend/PwCStationeryAPI/Controllers/DevController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PwCStationeryAPI.Data;

namespace PwCStationeryAPI.Controllers
{
    [ApiController]
    [Route("api/dev")]
    public class DevController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;

        public DevController(ApplicationDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        // Safety guard: only allow in Development
        private ActionResult? Guard()
        {
            if (!_env.IsDevelopment())
                return Forbid("Dev endpoints are disabled outside Development.");
            return null;
        }

        /// <summary>Re-run the seed (adds missing data; does not delete).</summary>
        [HttpPost("seed")]
        public IActionResult Seed()
        {
            var guard = Guard();
            if (guard is not null) return guard;

            DbInitializer.Initialize(_db);
            return Ok(new { message = "Seed completed." });
        }

        /// <summary>Delete ALL data (keeps schema) — use with care.</summary>
        [HttpPost("seed/clear")]
        public async Task<IActionResult> Clear()
        {
            var guard = Guard();
            if (guard is not null) return guard;

            // Ensure FK constraints are respected: delete dependents first
            // Order: StockLevels, Requests, Deliveries, Items, Suppliers, Categories, Offices
            _db.StockLevels.RemoveRange(_db.StockLevels);
            _db.Requests.RemoveRange(_db.Requests);
            _db.Deliveries.RemoveRange(_db.Deliveries);
            _db.Items.RemoveRange(_db.Items);
            _db.Suppliers.RemoveRange(_db.Suppliers);
            _db.Categories.RemoveRange(_db.Categories);
            _db.Offices.RemoveRange(_db.Offices);

            await _db.SaveChangesAsync();
            return Ok(new { message = "All data cleared (schema intact)." });
        }

        /// <summary>Clear then reseed everything (fresh demo data).</summary>
        [HttpPost("seed/reset")]
        public IActionResult Reset()
        {
            var guard = Guard();
            if (guard is not null) return guard;

            // Clear synchronously by reusing Clear() logic
            // (Run async synchronously for brevity)
            Clear().GetAwaiter().GetResult();

            DbInitializer.Initialize(_db);
            return Ok(new { message = "Database reset & reseeded." });
        }
    }
}

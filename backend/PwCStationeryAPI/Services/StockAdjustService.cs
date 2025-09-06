using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PwCStationeryAPI.Data;

namespace PwCStationeryAPI.Services
{
    public class StockAdjustService : IStockAdjustService
    {
        private readonly ApplicationDbContext _db;
        private readonly IAuditLogger _audit;

        public StockAdjustService(ApplicationDbContext db, IAuditLogger audit)
        {
            _db = db;
            _audit = audit;
        }

        public async Task AdjustAsync(int itemId, int officeId, int delta, string reason, CancellationToken ct = default)
        {
            // EF Core 7/8 atomic update (preferred)
            var affected = await _db.StockLevels
                .Where(s => s.ItemId == itemId && s.OfficeId == officeId
                            && (delta >= 0 || s.Quantity >= -delta)) // prevent negative stock when decrementing
                .ExecuteUpdateAsync(up => up.SetProperty(s => s.Quantity, s => s.Quantity + delta), ct);

            if (affected == 0)
                throw new InvalidOperationException($"Stock row not found or insufficient quantity for item {itemId} at office {officeId}");

            await _audit.LogAsync(null, "StockAdjusted", new { itemId, officeId, delta, reason });
        }
    }
}

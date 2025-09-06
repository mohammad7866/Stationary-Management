// backend/PwCStationeryAPI/Services/ReplenishmentService.cs
#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PwCStationeryAPI.Data;
using PwCStationeryAPI.Dtos.Replenishment;
using PwCStationeryAPI.Models;

namespace PwCStationeryAPI.Services
{
    public class ReplenishmentService : IReplenishmentService
    {
        private readonly ApplicationDbContext _db;
        private readonly IAuditLogger _audit;

        public ReplenishmentService(ApplicationDbContext db, IAuditLogger audit)
        {
            _db = db;
            _audit = audit;
        }

        public async Task<List<LowStockSuggestionDto>> GetSuggestionsAsync(string? office = null, int? minShortage = null)
        {
            // Base query + includes
            var q = _db.StockLevels
                .Include(s => s.Item)!.ThenInclude(i => i.Supplier)
                .Include(s => s.Office)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(office))
                q = q.Where(s => s.Office != null && s.Office.Name == office);

            // Low stock where threshold > 0 and qty <= threshold (NULL-safe for ReorderThreshold)
            q = q.Where(s =>
                (s.ReorderThreshold ?? 0) > 0 &&
                s.Quantity <= (s.ReorderThreshold ?? 0));

            var rows = await q.ToListAsync();

            // Project to DTOs with null-safe coalescing
            var list = rows.Select(s =>
            {
                var threshold = s.ReorderThreshold ?? 0;
                return new LowStockSuggestionDto
                {
                    StockLevelId = s.Id,
                    ItemId = s.ItemId,
                    ItemName = s.Item?.Name ?? $"Item {s.ItemId}",
                    OfficeId = s.OfficeId,
                    OfficeName = s.Office?.Name ?? string.Empty,
                    Quantity = s.Quantity,
                    ReorderThreshold = threshold,
                    SupplierId = s.Item?.SupplierId,          // stays nullable
                    SupplierName = s.Item?.Supplier?.Name,
                    SuggestedOrderQty = Math.Max(0, (threshold * 2) - s.Quantity)
                };
            }).ToList();

            if (minShortage.HasValue)
                list = list.Where(x => (x.ReorderThreshold - x.Quantity) >= minShortage.Value).ToList();

            return list;
        }

        public async Task<int> RaiseDeliveriesAsync(RaiseDeliveriesDto dto, string actorId)
        {
            if (dto?.Lines == null || dto.Lines.Count == 0)
                return 0;

            var expected = dto.ExpectedArrivalDateUtc ?? DateTime.UtcNow.AddDays(7);
            var now = DateTime.UtcNow;
            var created = 0;

            foreach (var line in dto.Lines)
            {
                if (line.Quantity <= 0)
                    continue;

                // Lookups (null-safe)
                var itemName = await _db.Items
                    .Where(i => i.Id == line.ItemId)
                    .Select(i => i.Name)
                    .FirstOrDefaultAsync() ?? $"Item {line.ItemId}";

                var officeName = await _db.Offices
                    .Where(o => o.Id == line.OfficeId)
                    .Select(o => o.Name)
                    .FirstOrDefaultAsync() ?? string.Empty;

                // Build Delivery (your Delivery model has no Quantity field)
                var delivery = new Delivery
                {
                    Product = itemName,
                    Office = officeName,
                    Status = "Ordered",
                    OrderedDateUtc = now,
                    ExpectedArrivalDateUtc = expected
                };

                // Only assign SupplierId when present to avoid int?/int mismatches
                if (line.SupplierId.HasValue)
                    delivery.SupplierId = line.SupplierId.Value;

                _db.Deliveries.Add(delivery);
                created++;
            }

            await _db.SaveChangesAsync();
            await _audit.LogAsync(actorId, "ReplenishmentRaised", new { requested = dto.Lines.Count, created });

            return created;
        }
    }
}

using System;

namespace PwCStationeryAPI.Dtos.Replenishment
{
    public class LowStockSuggestionDto
    {
        public int StockLevelId { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public int OfficeId { get; set; }
        public string OfficeName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int ReorderThreshold { get; set; }
        public int Shortage => Math.Max(0, ReorderThreshold - Quantity);
        public int SuggestedOrderQty { get; set; }
        public int? SupplierId { get; set; }
        public string? SupplierName { get; set; }
    }
}

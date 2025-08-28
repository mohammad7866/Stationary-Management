namespace PwCStationeryAPI.Models
{
    // Quantity of a specific Item at a specific Office
    public class StockLevel
    {
        public int Id { get; set; }

        public int ItemId { get; set; }
        public Item Item { get; set; } = null!;

        public int OfficeId { get; set; }
        public Office Office { get; set; } = null!;

        public int Quantity { get; set; }

        // Optional low-stock threshold for alerts
        public int? ReorderThreshold { get; set; }
    }
}

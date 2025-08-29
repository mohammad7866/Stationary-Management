using System.ComponentModel.DataAnnotations;

namespace PwCStationeryAPI.Models
{
    public class Delivery
    {
        public int Id { get; set; }

        [MaxLength(150)]
        public required string Product { get; set; }

        public int? SupplierId { get; set; }
        public Supplier? Supplier { get; set; }

        [MaxLength(120)]
        public required string Office { get; set; }

        // The date the order was placed
        public DateTime OrderedDateUtc { get; set; }

        public DateTime? ExpectedArrivalDateUtc { get; set; }

        public DateTime? ActualArrivalDateUtc { get; set; }

        [MaxLength(30)]
        public required string Status { get; set; } // Pending | On Time | Delayed | Received | Cancelled

        // Frozen delay after Received (Actual - Expected, in days)
        public int? FinalDelayDays { get; set; }

        // Convenience (live) if not frozen
        public int? ComputedDelayDays =>
            (ActualArrivalDateUtc.HasValue && ExpectedArrivalDateUtc.HasValue)
                ? (int?)(ActualArrivalDateUtc.Value.Date - ExpectedArrivalDateUtc.Value.Date).TotalDays
                : null;
    }
}

using System.ComponentModel.DataAnnotations;

namespace PwCStationeryAPI.Models
{
    public class Delivery
    {
        public int Id { get; set; }

        // Which product this delivery relates to (free text to match your UI; you can later link to ItemId)
        [MaxLength(150)]
        public required string Product { get; set; }

        // Supplier linkage is explicit to support historical delivery metrics by supplier
        public int? SupplierId { get; set; }
        public Supplier? Supplier { get; set; }

        // Office receiving the delivery (denormalized text to keep it simple for first pass)
        [MaxLength(120)]
        public required string Office { get; set; }

        public DateTime ScheduledDateUtc { get; set; }
        public DateTime? ArrivalDateUtc { get; set; }

        // "On Time" | "Pending" | "Delayed"
        [MaxLength(30)]
        public required string Status { get; set; }

        // Convenience: computed delay (in days) once arrived
        public int? ArrivalDelayDays =>
            ArrivalDateUtc.HasValue ? (int?)(ArrivalDateUtc.Value.Date - ScheduledDateUtc.Date).TotalDays : null;
    }
}

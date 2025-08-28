using System.ComponentModel.DataAnnotations;

namespace PwCStationeryAPI.Models
{
    public class Request
    {
        public int Id { get; set; }

        // For simplicity we store item name requested; later you may normalize to ItemId if needed
        [MaxLength(150)]
        public required string ItemName { get; set; }

        public int Quantity { get; set; }

        [MaxLength(200)]
        public required string Office { get; set; } // "London", "Manchester", ...

        [MaxLength(40)]
        public required string Status { get; set; } // "Pending" | "Approved" | "Rejected"

        [MaxLength(300)]
        public string? Purpose { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? DecisionAtUtc { get; set; } // when approved/rejected
    }
}

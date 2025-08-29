using System.ComponentModel.DataAnnotations;

namespace PwCStationeryAPI.Models
{
    public class AuditLog
    {
        public int Id { get; set; }

        [MaxLength(80)]
        public string Action { get; set; } = string.Empty; // e.g. "Request.Approve", "StockLevel.Update"

        [MaxLength(80)]
        public string Entity { get; set; } = string.Empty; // e.g. "Request", "StockLevel"

        [MaxLength(40)]
        public string? EntityId { get; set; }

        public DateTime WhenUtc { get; set; } = DateTime.UtcNow;

        [MaxLength(200)]
        public string? UserName { get; set; }  // set if you add auth later

        [MaxLength(64)]
        public string? ClientIp { get; set; }

        public string? BeforeJson { get; set; }
        public string? AfterJson { get; set; }
    }
}

// backend/PwCStationeryAPI/Models/AuditLog.cs
using System;

namespace PwCStationeryAPI.Models
{
    public class AuditLog
    {
        public long Id { get; set; }

        // Timestamps
        public DateTime TimestampUtc { get; set; } = DateTime.UtcNow; // for request logs
        public DateTime WhenUtc { get; set; } = DateTime.UtcNow;      // for your service logs

        // Caller
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public string? RolesCsv { get; set; }

        // Request metadata (from the global action filter)
        public string Method { get; set; } = "";
        public string Path { get; set; } = "";
        public string? Controller { get; set; }
        public string? Action { get; set; }
        public string? RouteValuesJson { get; set; }
        public string? QueryString { get; set; }
        public int StatusCode { get; set; }
        public int DurationMs { get; set; }
        public string? Ip { get; set; }
        public string? ClientIp { get; set; }    // <- your service expects this name
        public string? UserAgent { get; set; }

        // Change metadata (used by Services/AuditLogger.cs)
        public string? Entity { get; set; }      // e.g., "Item", "Request"
        public string? EntityId { get; set; }    // e.g., "42"
        public string? BeforeJson { get; set; }  // serialized "before" state
        public string? AfterJson { get; set; }   // serialized "after" state
    }
}

// backend/PwCStationeryAPI/Models/DTOs/AuditLogDto.cs
namespace PwCStationeryAPI.Models.DTOs
{
    public class AuditLogDto
    {
        public long Id { get; set; }
        public DateTime Timestamp { get; set; }   // matches your entity's DateTime
        public string? ActorId { get; set; }
        public string? ActorName { get; set; }
        public string Action { get; set; } = "";
        public string EntityType { get; set; } = "";
        public string? EntityId { get; set; }
        public string? Details { get; set; }
    }
}

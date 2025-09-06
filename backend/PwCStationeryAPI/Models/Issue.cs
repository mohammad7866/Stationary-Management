using System.Text.Json.Serialization;

namespace PwCStationeryAPI.Models
{
    public class Issue
    {
        public int Id { get; set; }
        public int RequestId { get; set; }
        public Request? Request { get; set; }
        public string IssuedByUserId { get; set; } = string.Empty;
        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
        public ICollection<IssueLine> Lines { get; set; } = new List<IssueLine>();
    }

    public class IssueLine
    {
        public int Id { get; set; }
        public int IssueId { get; set; }

        [JsonIgnore]                 // ← prevents Issue -> Lines -> Issue loop
        public Issue? Issue { get; set; }

        public int ItemId { get; set; }
        public Item? Item { get; set; }
        public int Quantity { get; set; }
    }
}

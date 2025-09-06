using System.Text.Json.Serialization;

namespace PwCStationeryAPI.Models
{
    public class Return
    {
        public int Id { get; set; }
        public int IssueId { get; set; }
        public Issue? Issue { get; set; }
        public string ReturnedByUserId { get; set; } = string.Empty;
        public DateTime ReturnedAt { get; set; } = DateTime.UtcNow;
        public ICollection<ReturnLine> Lines { get; set; } = new List<ReturnLine>();
    }

    public class ReturnLine
    {
        public int Id { get; set; }
        public int ReturnId { get; set; }

        [JsonIgnore]                 // ← prevents Return -> Lines -> Return loop
        public Return? Return { get; set; }

        public int ItemId { get; set; }
        public Item? Item { get; set; }
        public int Quantity { get; set; }
    }
}

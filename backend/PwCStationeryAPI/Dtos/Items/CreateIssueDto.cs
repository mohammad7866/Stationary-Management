using System.Collections.Generic;

namespace PwCStationeryAPI.Dtos.Items
{
    public class CreateIssueDto
    {
        public int RequestId { get; set; }
        public List<IssueLineDto> Lines { get; set; } = new();
        public string? IdempotencyKey { get; set; }
    }

    public class IssueLineDto
    {
        public int ItemId { get; set; }
        public int Quantity { get; set; }
    }
}

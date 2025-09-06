using System.Collections.Generic;

namespace PwCStationeryAPI.Dtos.Items
{
    public class CreateReturnDto
    {
        public int IssueId { get; set; }
        public List<ReturnLineDto> Lines { get; set; } = new();
    }

    public class ReturnLineDto
    {
        public int ItemId { get; set; }
        public int Quantity { get; set; }
    }
}

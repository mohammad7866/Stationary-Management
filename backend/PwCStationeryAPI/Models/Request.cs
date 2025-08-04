using System;

namespace PwCStationeryAPI.Models
{
    public class Request
    {
        public int Id { get; set; }
        public string ItemName { get; set; }
        public int Quantity { get; set; }
        public string Office { get; set; }
        public string Status { get; set; } // e.g. Pending, Approved, Rejected
        public DateTime RequestDate { get; set; }
    }
}

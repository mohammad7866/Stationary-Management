using System;

namespace PwCStationeryAPI.Models
{
    public class Delivery
    {
        public int Id { get; set; }
        public string Product { get; set; }
        public string Supplier { get; set; }
        public string Office { get; set; }
        public DateTime ScheduledDate { get; set; }
        public DateTime? ArrivalDate { get; set; }
        public string Status { get; set; } // On Time, Pending, Delayed
    }
}

using System;
using System.Collections.Generic;

namespace PwCStationeryAPI.Dtos.Replenishment
{
    public class RaiseDeliveriesDto
    {
        public List<RaiseDeliveryLineDto> Lines { get; set; } = new();
        public DateTime? ExpectedArrivalDateUtc { get; set; } // optional default +7d
    }

    public class RaiseDeliveryLineDto
    {
        public int StockLevelId { get; set; }
        public int ItemId { get; set; }
        public int OfficeId { get; set; }
        public int? SupplierId { get; set; }   // <-- nullable to match Delivery.SupplierId
        public int Quantity { get; set; }
    }
}

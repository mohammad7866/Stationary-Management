namespace PwCStationeryAPI.Dtos.Deliveries
{
    public class ReadDeliveryDto
    {
        public int Id { get; set; }

        public string Product { get; set; } = string.Empty;
        public string Office { get; set; } = string.Empty;

        public int? SupplierId { get; set; }
        public string? SupplierName { get; set; }

        public DateTime OrderedDateUtc { get; set; }
        public DateTime? ExpectedArrivalDateUtc { get; set; }
        public DateTime? ActualArrivalDateUtc { get; set; }

        public int? ArrivalDelayDays { get; set; }

        public string Status { get; set; } = "Pending";
    }
}

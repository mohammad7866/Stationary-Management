using System.ComponentModel.DataAnnotations;

namespace PwCStationeryAPI.Dtos.Deliveries
{
    public class CreateDeliveryDto
    {
        [Required, MaxLength(150)]
        public string Product { get; set; } = string.Empty;

        [Required, MaxLength(120)]
        public string Office { get; set; } = string.Empty;

        public int? SupplierId { get; set; }

        [Required] // Order date
        public DateTime OrderedDateUtc { get; set; }

        [Required] // Expected arrival
        public DateTime ExpectedArrivalDateUtc { get; set; }

        // Only allowed if creating directly as Received
        public DateTime? ActualArrivalDateUtc { get; set; }

        [MaxLength(30)]
        public string? Status { get; set; } = "Pending";
    }
}

using System.ComponentModel.DataAnnotations;

namespace PwCStationeryAPI.Dtos.Deliveries
{
    public class UpdateDeliveryDto
    {
        public int? SupplierId { get; set; }
        public DateTime? OrderedDateUtc { get; set; }
        public DateTime? ExpectedArrivalDateUtc { get; set; }

        // Provide when marking Received (or correcting)
        public DateTime? ActualArrivalDateUtc { get; set; }

        [Required, MaxLength(30)]
        public string Status { get; set; } = "Pending";
    }
}

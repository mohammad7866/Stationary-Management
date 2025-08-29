using System.ComponentModel.DataAnnotations;

namespace PwCStationeryAPI.Dtos.Deliveries
{
    public class UpdateDeliveryStatusDto
    {
        [Required, MaxLength(30)]
        public string Status { get; set; } = "Pending";
    }
}

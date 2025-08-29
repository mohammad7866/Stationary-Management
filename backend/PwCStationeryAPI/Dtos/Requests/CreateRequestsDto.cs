using System.ComponentModel.DataAnnotations;

namespace PwCStationeryAPI.Dtos.Requests
{
    public class CreateRequestDto
    {
        [Required, MaxLength(120)]
        public string ItemName { get; set; } = string.Empty;

        [Required, MaxLength(80)]
        public string Office { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required, MaxLength(200)]
        public string Purpose { get; set; } = string.Empty;

        [MaxLength(40)]
        public string? Status { get; set; } = "Pending";
    }
}

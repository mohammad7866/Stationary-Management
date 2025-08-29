using System.ComponentModel.DataAnnotations;

namespace PwCStationeryAPI.Dtos.Requests
{
    // Full edit (quantity/purpose/status). Adjust if you want to allow renaming Item/Office.
    public class UpdateRequestDto
    {
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required, MaxLength(200)]
        public string Purpose { get; set; } = string.Empty;

        [Required, MaxLength(40)]
        public string Status { get; set; } = "Pending";
    }
}

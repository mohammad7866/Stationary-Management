using System.ComponentModel.DataAnnotations;

namespace PwCStationeryAPI.Dtos.Requests
{
    public class UpdateRequestStatusDto
    {
        [Required, MaxLength(40)]
        public string Status { get; set; } = "Pending";
    }
}

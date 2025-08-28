using System.ComponentModel.DataAnnotations;

namespace PwCStationeryAPI.Dtos.Items
{
    /// <summary>
    /// Payload to update an existing Item.
    /// </summary>
    public class UpdateItemDto
    {
        [Required, MaxLength(80)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public int CategoryId { get; set; }

        public int? SupplierId { get; set; }
    }
}

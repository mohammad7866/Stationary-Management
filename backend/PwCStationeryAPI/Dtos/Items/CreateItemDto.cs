using System.ComponentModel.DataAnnotations;

namespace PwCStationeryAPI.Dtos.Items
{
    /// <summary>
    /// Payload to create a new Item.
    /// </summary>
    public class CreateItemDto
    {
        [Required, MaxLength(80)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public int CategoryId { get; set; }

        /// <summary>
        /// Optional supplier reference. Leave null if not applicable.
        /// </summary>
        public int? SupplierId { get; set; }
    }
}

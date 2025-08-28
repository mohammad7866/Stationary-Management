using System.ComponentModel.DataAnnotations;

namespace PwCStationeryAPI.Dtos.Suppliers
{
    /// <summary>Create a new supplier.</summary>
    public class CreateSupplierDto
    {
        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [EmailAddress, MaxLength(150)]
        public string? ContactEmail { get; set; }

        [MaxLength(40)]
        public string? Phone { get; set; }
    }
}

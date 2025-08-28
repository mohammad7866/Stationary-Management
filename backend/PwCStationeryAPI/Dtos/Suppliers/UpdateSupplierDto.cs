using System.ComponentModel.DataAnnotations;

namespace PwCStationeryAPI.Dtos.Suppliers
{
    /// <summary>Update an existing supplier.</summary>
    public class UpdateSupplierDto
    {
        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [EmailAddress, MaxLength(150)]
        public string? ContactEmail { get; set; }

        [MaxLength(40)]
        public string? Phone { get; set; }
    }
}

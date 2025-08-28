using System.ComponentModel.DataAnnotations;

namespace PwCStationeryAPI.Models
{
    public class Supplier
    {
        public int Id { get; set; }

        [MaxLength(150)]
        public required string Name { get; set; }

        [MaxLength(150)]
        public string? ContactEmail { get; set; }

        [MaxLength(40)]
        public string? Phone { get; set; }
    }
}

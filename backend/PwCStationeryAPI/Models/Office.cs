using System.ComponentModel.DataAnnotations;

namespace PwCStationeryAPI.Models
{
    public class Office
    {
        public int Id { get; set; }

        [MaxLength(120)]
        public required string Name { get; set; }   // e.g., "London"

        [MaxLength(200)]
        public required string Location { get; set; } // freeform, e.g., "1 Embankment, London"
    }
}

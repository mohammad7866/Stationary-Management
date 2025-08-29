using System.ComponentModel.DataAnnotations;

namespace PwCStationeryAPI.Dtos.StockLevels
{
    public class CreateStockLevelDto
    {
        [Required] public int ItemId { get; set; }
        [Required] public int OfficeId { get; set; }

        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        [Range(0, int.MaxValue)]
        public int ReorderThreshold { get; set; }
    }
}

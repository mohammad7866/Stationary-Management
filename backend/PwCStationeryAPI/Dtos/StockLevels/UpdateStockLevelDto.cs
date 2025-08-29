using System.ComponentModel.DataAnnotations;

namespace PwCStationeryAPI.Dtos.StockLevels
{
    public class UpdateStockLevelDto
    {
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        [Range(0, int.MaxValue)]
        public int ReorderThreshold { get; set; }
    }
}

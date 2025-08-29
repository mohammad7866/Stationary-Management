namespace PwCStationeryAPI.Dtos.StockLevels
{
    public class ReadStockLevelDto
    {
        public int Id { get; set; }

        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;

        public int OfficeId { get; set; }
        public string OfficeName { get; set; } = string.Empty;

        public int Quantity { get; set; }
        public int? ReorderThreshold { get; set; }
        public bool IsLow { get; set; }

    }
}

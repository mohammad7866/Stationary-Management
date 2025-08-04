namespace PwCStationeryAPI.Models
{
    public class StockLevel
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public Item Item { get; set; }
        public int OfficeId { get; set; }
        public Office Office { get; set; }
        public int Quantity { get; set; }
    }
}

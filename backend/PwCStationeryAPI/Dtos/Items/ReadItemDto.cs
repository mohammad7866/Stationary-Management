namespace PwCStationeryAPI.Dtos.Items
{
    /// <summary>
    /// Read model for returning Items to clients.
    /// </summary>
    public class ReadItemDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;

        public int? SupplierId { get; set; }
        public string? SupplierName { get; set; }
    }
}

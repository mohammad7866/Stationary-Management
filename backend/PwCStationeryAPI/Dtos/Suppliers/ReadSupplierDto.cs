namespace PwCStationeryAPI.Dtos.Suppliers
{
    /// <summary>Supplier read model.</summary>
    public class ReadSupplierDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ContactEmail { get; set; }
        public string? Phone { get; set; }
    }
}

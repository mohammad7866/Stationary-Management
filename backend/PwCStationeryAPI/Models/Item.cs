namespace PwCStationeryAPI.Models
{
    public class Item
    {
        public int Id { get; set; }
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;

        // Foreign Keys
        public int CategoryId { get; set; }
        public Category Category { get; set; } = default!;

        public int? SupplierId { get; set; }
        public Supplier? Supplier { get; set; }
    }
}

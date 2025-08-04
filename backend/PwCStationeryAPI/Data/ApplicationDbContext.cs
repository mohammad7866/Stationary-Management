using Microsoft.EntityFrameworkCore;
using PwCStationeryAPI.Models;

namespace PwCStationeryAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Item> Items { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Office> Offices { get; set; }
        public DbSet<StockLevel> StockLevels { get; set; }
        public DbSet<Request> Requests { get; set; }
        public DbSet<Delivery> Deliveries { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}

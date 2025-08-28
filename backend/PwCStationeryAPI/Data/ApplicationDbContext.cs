using Microsoft.EntityFrameworkCore;
using PwCStationeryAPI.Models;

namespace PwCStationeryAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Supplier> Suppliers => Set<Supplier>();
        public DbSet<Office> Offices => Set<Office>();
        public DbSet<Item> Items => Set<Item>();
        public DbSet<StockLevel> StockLevels => Set<StockLevel>();
        public DbSet<Request> Requests => Set<Request>();
        public DbSet<Delivery> Deliveries => Set<Delivery>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Category
            modelBuilder.Entity<Category>()
                .HasIndex(c => c.Name)
                .IsUnique();

            // Supplier
            modelBuilder.Entity<Supplier>()
                .HasIndex(s => s.Name);

            // Office
            modelBuilder.Entity<Office>()
                .HasIndex(o => o.Name)
                .IsUnique();

            // Item
            modelBuilder.Entity<Item>()
                .HasOne(i => i.Category)
                .WithMany()
                .HasForeignKey(i => i.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Item>()
                .HasOne(i => i.Supplier)
                .WithMany()
                .HasForeignKey(i => i.SupplierId)
                .OnDelete(DeleteBehavior.SetNull);

            // Useful uniqueness: same item name under same supplier considered unique
            modelBuilder.Entity<Item>()
                .HasIndex(i => new { i.Name, i.SupplierId })
                .IsUnique();

            // StockLevel: unique per (Item, Office)
            modelBuilder.Entity<StockLevel>()
                .HasIndex(s => new { s.ItemId, s.OfficeId })
                .IsUnique();

            modelBuilder.Entity<StockLevel>()
                .HasOne(s => s.Item)
                .WithMany()
                .HasForeignKey(s => s.ItemId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StockLevel>()
                .HasOne(s => s.Office)
                .WithMany()
                .HasForeignKey(s => s.OfficeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Request: keep it lightweight for now (text Office + ItemName)
            modelBuilder.Entity<Request>()
                .Property(r => r.Status)
                .HasDefaultValue("Pending");

            modelBuilder.Entity<Request>()
                .HasIndex(r => new { r.Office, r.Status });

            // Delivery
            modelBuilder.Entity<Delivery>()
                .HasOne(d => d.Supplier)
                .WithMany()
                .HasForeignKey(d => d.SupplierId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Delivery>()
                .HasIndex(d => new { d.Office, d.ScheduledDateUtc });

            modelBuilder.Entity<Delivery>()
                .HasIndex(d => new { d.SupplierId, d.ScheduledDateUtc });
        }
    }
}

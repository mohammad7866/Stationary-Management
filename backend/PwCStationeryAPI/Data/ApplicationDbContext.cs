// backend/PwCStationeryAPI/Data/ApplicationDbContext.cs
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PwCStationeryAPI.Models;

namespace PwCStationeryAPI.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Item> Items => Set<Item>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Supplier> Suppliers => Set<Supplier>();
        public DbSet<Office> Offices => Set<Office>();
        public DbSet<StockLevel> StockLevels => Set<StockLevel>();
        public DbSet<Request> Requests => Set<Request>();
        public DbSet<Delivery> Deliveries => Set<Delivery>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ---------- Item ----------
            modelBuilder.Entity<Item>(b =>
            {
                b.Property(i => i.Name).HasMaxLength(150).IsRequired();
                b.Property(i => i.Description).HasMaxLength(500);
                b.HasOne(i => i.Category).WithMany().HasForeignKey(i => i.CategoryId).OnDelete(DeleteBehavior.Restrict);
                b.HasOne(i => i.Supplier).WithMany().HasForeignKey(i => i.SupplierId).OnDelete(DeleteBehavior.SetNull);
                b.HasIndex(i => i.Name);
            });

            // ---------- Category ----------
            modelBuilder.Entity<Category>(b =>
            {
                b.Property(c => c.Name).HasMaxLength(100).IsRequired();
                b.HasIndex(c => c.Name);
            });

            // ---------- Supplier ----------
            modelBuilder.Entity<Supplier>(b =>
            {
                b.Property(s => s.Name).HasMaxLength(120).IsRequired();
                b.Property(s => s.ContactEmail).HasMaxLength(200);
                b.Property(s => s.Phone).HasMaxLength(50);
                b.HasIndex(s => s.Name);
            });

            // ---------- Office ----------
            modelBuilder.Entity<Office>(b =>
            {
                b.Property(o => o.Name).HasMaxLength(120).IsRequired();
                b.Property(o => o.Location).HasMaxLength(120);
                b.HasIndex(o => o.Name);
            });

            // ---------- StockLevel ----------
            modelBuilder.Entity<StockLevel>(b =>
            {
                b.HasOne(s => s.Item).WithMany().HasForeignKey(s => s.ItemId).OnDelete(DeleteBehavior.Cascade);
                b.HasOne(s => s.Office).WithMany().HasForeignKey(s => s.OfficeId).OnDelete(DeleteBehavior.Cascade);
                b.Property(s => s.ReorderThreshold).HasDefaultValue(0);
                b.HasIndex(s => new { s.ItemId, s.OfficeId }).IsUnique();
            });

            // ---------- Request ----------
            modelBuilder.Entity<Request>(b =>
            {
                b.Property(r => r.ItemName).HasMaxLength(150).IsRequired();
                b.Property(r => r.Office).HasMaxLength(120).IsRequired();
                b.Property(r => r.Status).HasMaxLength(30).HasDefaultValue("Pending");
                b.HasIndex(r => new { r.Office, r.Status });
            });

            // ---------- Delivery ----------
            modelBuilder.Entity<Delivery>(b =>
            {
                b.Property(d => d.Product).HasMaxLength(150).IsRequired();
                b.Property(d => d.Office).HasMaxLength(120).IsRequired();
                b.Property(d => d.Status).HasMaxLength(30).IsRequired();

                b.Property(d => d.OrderedDateUtc).IsRequired();
                b.Property(d => d.ExpectedArrivalDateUtc);
                b.Property(d => d.ActualArrivalDateUtc);
                b.Property(d => d.FinalDelayDays);

                b.HasOne(d => d.Supplier).WithMany().HasForeignKey(d => d.SupplierId).OnDelete(DeleteBehavior.SetNull);

                b.HasIndex(d => d.OrderedDateUtc);
                b.HasIndex(d => new { d.Office, d.Status });
            });

            // ---------- AuditLog ----------
            modelBuilder.Entity<AuditLog>(b =>
            {
                b.Property(a => a.Action).HasMaxLength(80).IsRequired();
                b.Property(a => a.Entity).HasMaxLength(80).IsRequired();
                b.Property(a => a.EntityId).HasMaxLength(40);
                b.Property(a => a.UserName).HasMaxLength(200);
                b.Property(a => a.ClientIp).HasMaxLength(64);
                b.Property(a => a.WhenUtc).IsRequired();
                b.HasIndex(a => a.WhenUtc);
                b.HasIndex(a => new { a.Entity, a.EntityId });
            });
        }
    }
}

using PwCStationeryAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace PwCStationeryAPI.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            // Ensure DB exists
            context.Database.EnsureCreated();

            // 1) Categories
            EnsureCategory(context, "Core");
            EnsureCategory(context, "Special");
            EnsureCategory(context, "Printed");
            context.SaveChanges();

            // 2) Offices
            EnsureOffice(context, "London", "UK");
            EnsureOffice(context, "Manchester", "UK");
            EnsureOffice(context, "Birmingham", "UK");
            context.SaveChanges();

            // 3) Suppliers
            EnsureSupplier(context, "Office Depot", "orders@officedepot.com", "0123456789");
            EnsureSupplier(context, "Staples", "sales@staples.co.uk", "02079460000");
            EnsureSupplier(context, "Ryman", "info@ryman.co.uk", "0161222333");
            context.SaveChanges();

            // Lookups (safe after ensures)
            var catCore = context.Categories.First(c => c.Name == "Core");
            var catSpecial = context.Categories.First(c => c.Name == "Special");
            var catPrinted = context.Categories.First(c => c.Name == "Printed");

            var supDepot = context.Suppliers.First(s => s.Name == "Office Depot");
            var supStaples = context.Suppliers.First(s => s.Name == "Staples");
            var supRyman = context.Suppliers.First(s => s.Name == "Ryman");

            var offLondon = context.Offices.First(o => o.Name == "London");
            var offManchester = context.Offices.First(o => o.Name == "Manchester");
            var offBirmingham = context.Offices.First(o => o.Name == "Birmingham");

            // 4) Items
            EnsureItem(context, "Printer Paper A4", "500 sheets per pack", catCore.Id, supDepot.Id);
            EnsureItem(context, "Pens - Black", "Box of 20 pens", catCore.Id, supStaples.Id);
            EnsureItem(context, "Notebooks", "A4 ruled notebooks", catSpecial.Id, supRyman.Id);
            context.SaveChanges();

            var itemPaper = context.Items.First(i => i.Name == "Printer Paper A4");
            var itemPens = context.Items.First(i => i.Name == "Pens - Black");
            var itemNB = context.Items.First(i => i.Name == "Notebooks");

            // 5) StockLevels (unique per Item+Office)
            EnsureStock(context, itemPaper.Id, offLondon.Id, quantity: 200, reorderThreshold: 50);
            EnsureStock(context, itemPaper.Id, offManchester.Id, quantity: 120, reorderThreshold: 40);
            EnsureStock(context, itemPens.Id, offLondon.Id, quantity: 100, reorderThreshold: 30);
            EnsureStock(context, itemPens.Id, offBirmingham.Id, quantity: 60, reorderThreshold: 20);
            EnsureStock(context, itemNB.Id, offManchester.Id, quantity: 35, reorderThreshold: 10);
            context.SaveChanges();

            // 6) Deliveries (UTC + SupplierId)
            EnsureDelivery(
                context,
                product: "Printer Paper A4",
                supplierId: supDepot.Id,
                office: "London",
                scheduledUtc: new DateTime(2025, 8, 5, 0, 0, 0, DateTimeKind.Utc),
                arrivalUtc: new DateTime(2025, 8, 4, 0, 0, 0, DateTimeKind.Utc),
                status: "On Time"
            );

            EnsureDelivery(
                context,
                product: "Pens - Black",
                supplierId: supStaples.Id,
                office: "Manchester",
                scheduledUtc: new DateTime(2025, 8, 6, 0, 0, 0, DateTimeKind.Utc),
                arrivalUtc: null,
                status: "Pending"
            );

            EnsureDelivery(
                context,
                product: "Notebooks",
                supplierId: supRyman.Id,
                office: "Birmingham",
                scheduledUtc: new DateTime(2025, 8, 3, 0, 0, 0, DateTimeKind.Utc),
                arrivalUtc: new DateTime(2025, 8, 5, 0, 0, 0, DateTimeKind.Utc),
                status: "Delayed"
            );
            context.SaveChanges();

            // 7) Requests
            EnsureRequest(
                context,
                itemName: "Pens - Black",
                office: "London",
                quantity: 10,
                status: "Approved",
                createdAtUtc: DateTime.UtcNow.AddDays(-2),
                decisionAtUtc: DateTime.UtcNow.AddDays(-2)
            );

            EnsureRequest(
                context,
                itemName: "Notebooks",
                office: "Manchester",
                quantity: 5,
                status: "Pending",
                createdAtUtc: DateTime.UtcNow.AddDays(-1),
                decisionAtUtc: null
            );
            context.SaveChanges();
        }

        // ---------- Idempotent helpers ----------

        private static void EnsureCategory(ApplicationDbContext db, string name)
        {
            if (!db.Categories.Any(c => c.Name == name))
                db.Categories.Add(new Category { Name = name });
        }

        private static void EnsureOffice(ApplicationDbContext db, string name, string location)
        {
            if (!db.Offices.Any(o => o.Name == name))
                db.Offices.Add(new Office { Name = name, Location = location });
        }

        private static void EnsureSupplier(ApplicationDbContext db, string name, string email, string phone)
        {
            if (!db.Suppliers.Any(s => s.Name == name))
                db.Suppliers.Add(new Supplier { Name = name, ContactEmail = email, Phone = phone });
        }

        private static void EnsureItem(ApplicationDbContext db, string name, string description, int categoryId, int supplierId)
        {
            if (!db.Items.Any(i => i.Name == name))
            {
                db.Items.Add(new Item
                {
                    Name = name,
                    Description = description,
                    CategoryId = categoryId,
                    SupplierId = supplierId
                });
            }
        }

        private static void EnsureStock(ApplicationDbContext db, int itemId, int officeId, int quantity, int reorderThreshold)
        {
            var existing = db.StockLevels.FirstOrDefault(s => s.ItemId == itemId && s.OfficeId == officeId);
            if (existing == null)
            {
                db.StockLevels.Add(new StockLevel
                {
                    ItemId = itemId,
                    OfficeId = officeId,
                    Quantity = quantity,
                    ReorderThreshold = reorderThreshold
                });
            }
            else
            {
                // keep values in sync on reseed
                existing.Quantity = quantity;
                existing.ReorderThreshold = reorderThreshold;
            }
        }

        private static void EnsureDelivery(ApplicationDbContext db, string product, int supplierId, string office, DateTime scheduledUtc, DateTime? arrivalUtc, string status)
        {
            var existing = db.Deliveries.FirstOrDefault(d =>
                d.Product == product &&
                d.SupplierId == supplierId &&
                d.Office == office &&
                d.ScheduledDateUtc == scheduledUtc
            );

            if (existing == null)
            {
                db.Deliveries.Add(new Delivery
                {
                    Product = product,
                    SupplierId = supplierId,
                    Office = office,
                    ScheduledDateUtc = scheduledUtc,
                    ArrivalDateUtc = arrivalUtc,
                    Status = status
                });
            }
            else
            {
                existing.ArrivalDateUtc = arrivalUtc;
                existing.Status = status;
            }
        }

        private static void EnsureRequest(ApplicationDbContext db, string itemName, string office, int quantity, string status, DateTime createdAtUtc, DateTime? decisionAtUtc)
        {
            var existing = db.Requests.FirstOrDefault(r =>
                r.ItemName == itemName &&
                r.Office == office &&
                r.Quantity == quantity &&
                r.CreatedAtUtc.Date == createdAtUtc.Date
            );

            if (existing == null)
            {
                db.Requests.Add(new Request
                {
                    ItemName = itemName,
                    Office = office,
                    Quantity = quantity,
                    Status = status,
                    CreatedAtUtc = createdAtUtc,
                    DecisionAtUtc = decisionAtUtc
                });
            }
            else
            {
                existing.Status = status;
                existing.DecisionAtUtc = decisionAtUtc;
            }
        }
    }
}

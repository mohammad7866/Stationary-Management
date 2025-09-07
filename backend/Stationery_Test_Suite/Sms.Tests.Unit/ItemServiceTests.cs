using System;
using System.Linq;

using FluentAssertions;
using Xunit;

namespace Sms.Tests.Unit;

public class ItemServiceTests
{
    [Fact]
    public void DecrementStock_Should_Decrease_When_Sufficient()
    {
        using var db = DbUtil.InMemory();
        var audit = new AuditService(db);
        var inventory = new InventoryService(db, audit);
        var item = new Item { Name = "Pens", Stock = 50, ReorderThreshold = 20 };
        db.Items.Add(item); db.SaveChanges();

        inventory.DecrementStock(item.Id, 10, actorId: 99);

        db.Items.Find(item.Id)!.Stock.Should().Be(40);
        db.AuditLogs.Should().ContainSingle(x => x.Action == "Issue" && x.Entity == "Item" && x.EntityId == item.Id);
    }

    [Fact]
    public void DecrementStock_Should_Throw_When_Insufficient()
    {
        using var db = DbUtil.InMemory();
        var audit = new AuditService(db);
        var inventory = new InventoryService(db, audit);
        var item = new Item { Name = "Paper", Stock = 5, ReorderThreshold = 20 };
        db.Items.Add(item); db.SaveChanges();

        var act = () => inventory.DecrementStock(item.Id, 6, actorId: 1);
        act.Should().Throw<InvalidOperationException>().WithMessage("*Insufficient stock*");
        db.Items.Find(item.Id)!.Stock.Should().Be(5);
    }
}

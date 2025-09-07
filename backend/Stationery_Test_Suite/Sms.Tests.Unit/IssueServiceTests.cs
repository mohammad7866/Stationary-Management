using System;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace Sms.Tests.Unit;

public class IssueServiceTests
{
    [Fact]
    public void Issue_Should_Decrement_Stock_And_Mark_Issued_And_Audit()
    {
        using var db = DbUtil.InMemory();
        var audit = new AuditService(db);
        var inventory = new InventoryService(db, audit);
        var svc = new IssueService(db, inventory, audit);
        var item = new Item{ Name = "Staplers", Stock = 10, ReorderThreshold = 3 };
        db.Items.Add(item);
        var req = new Request{ ItemId = item.Id, Quantity = 4, Status = RequestStatus.Approved, RequestedById = 7 };
        db.Requests.Add(req);
        db.SaveChanges();

        svc.Issue(req.Id, actorId: 99);

        db.Items.Find(item.Id)!.Stock.Should().Be(6);
        db.Requests.Find(req.Id)!.Status.Should().Be(RequestStatus.Issued);
        db.AuditLogs.Count().Should().BeGreaterOrEqualTo(2);
    }

    [Fact]
    public void Issue_Should_Fail_If_Not_Approved()
    {
        using var db = DbUtil.InMemory();
        var audit = new AuditService(db);
        var inventory = new InventoryService(db, audit);
        var svc = new IssueService(db, inventory, audit);
        var item = new Item{ Name = "Paper", Stock = 5, ReorderThreshold = 2 };
        db.Items.Add(item);
        var req = new Request{ ItemId = item.Id, Quantity = 6, Status = RequestStatus.Pending, RequestedById = 7 };
        db.Requests.Add(req); db.SaveChanges();

        var act = () => svc.Issue(req.Id, actorId: 1);
        act.Should().Throw<InvalidOperationException>().WithMessage("*not approved*");
    }
}

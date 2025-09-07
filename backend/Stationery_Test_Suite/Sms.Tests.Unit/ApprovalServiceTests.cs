using System;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace Sms.Tests.Unit;

public class ApprovalServiceTests
{
    [Fact]
    public void Approve_Should_Transition_Pending_To_Approved_And_Audit()
    {
        using var db = DbUtil.InMemory();
        var audit = new AuditService(db);
        var req = new Request{ ItemId = 1, Quantity = 2, RequestedById = 5, Status = RequestStatus.Pending };
        db.Requests.Add(req); db.SaveChanges();

        var svc = new ApprovalService(db, audit);
        var app = svc.Approve(req.Id, approverId: 42);

        db.Requests.Find(req.Id)!.Status.Should().Be(RequestStatus.Approved);
        app.Approved.Should().BeTrue();
        db.AuditLogs.Should().ContainSingle(l => l.Action == "Approve" && l.EntityId == req.Id);
    }

    [Fact]
    public void Approve_Should_Fail_For_NonPending()
    {
        using var db = DbUtil.InMemory();
        var audit = new AuditService(db);
        var req = new Request{ ItemId = 1, Quantity = 2, RequestedById = 5, Status = RequestStatus.Approved };
        db.Requests.Add(req); db.SaveChanges();

        var svc = new ApprovalService(db, audit);
        var act = () => svc.Approve(req.Id, approverId: 42);

        act.Should().Throw<InvalidOperationException>().WithMessage("*Invalid transition*");
    }
}

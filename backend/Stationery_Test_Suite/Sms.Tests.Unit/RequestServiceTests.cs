using System;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace Sms.Tests.Unit;

public class RequestServiceTests
{
    [Fact]
    public void Create_Should_Create_Pending_Request_And_Audit()
    {
        using var db = DbUtil.InMemory();
        var audit = new AuditService(db);
        var svc = new RequestService(db, audit);

        var req = svc.Create(itemId: 1, qty: 3, requestedById: 7);

        req.Status.Should().Be(RequestStatus.Pending);
        db.Requests.Count().Should().Be(1);
        db.AuditLogs.Should().ContainSingle(l => l.Action == "CreateRequest" && l.Entity == "Request" && l.ActorId == 7);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void Create_Should_Reject_NonPositive_Qty(int qty)
    {
        using var db = DbUtil.InMemory();
        var audit = new AuditService(db);
        var svc = new RequestService(db, audit);

        var act = () => svc.Create(itemId: 1, qty: qty, requestedById: 7);
        act.Should().Throw<ArgumentOutOfRangeException>();
        db.Requests.Should().BeEmpty();
    }
}

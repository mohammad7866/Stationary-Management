using System;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace Sms.Tests.Unit;

public class AuditServiceTests
{
    [Fact]
    public void Log_Should_Write_Audit_Row_With_All_Fields()
    {
        using var db = DbUtil.InMemory();
        var audit = new AuditService(db);

        audit.Log(actorId: 1, action: "Create", entity: "Item", entityId: 11);

        var row = db.AuditLogs.Single();
        row.ActorId.Should().Be(1);
        row.Action.Should().Be("Create");
        row.Entity.Should().Be("Item");
        row.EntityId.Should().Be(11);
        row.Timestamp.Should().BeOnOrAfter(DateTimeOffset.UtcNow.AddMinutes(-1));
    }
}

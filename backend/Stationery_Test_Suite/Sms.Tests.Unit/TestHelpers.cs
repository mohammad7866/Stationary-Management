using System;            // needed for exceptions & DateTimeOffset
using System.Linq;       // needed for Count(), Single(), etc.
using FluentAssertions;
using Xunit;
using Microsoft.EntityFrameworkCore;

namespace Sms.Tests.Unit;

public enum RequestStatus { Pending=0, Approved=1, Rejected=2, Issued=3 }

// --- Minimal model types to compile tests ---
// If you already have these, delete these and import your real models.
public class User { public int Id {get; set;} public string Role {get; set;} = "User"; }
public class Item { public int Id {get; set;} public string Name {get; set;} = ""; public int Stock {get; set;} public int ReorderThreshold {get; set;} }
public class Request { public int Id {get; set;} public int ItemId {get; set;} public int Quantity {get; set;} public RequestStatus Status {get; set;} = RequestStatus.Pending; public int RequestedById {get; set;} }
public class Approval { public int Id {get; set;} public int RequestId {get; set;} public int ApprovedById {get; set;} public bool Approved {get; set;} public DateTimeOffset Timestamp {get; set;} = DateTimeOffset.UtcNow; }
public class AuditLog { public int Id {get; set;} public string Action {get; set;} = ""; public string Entity {get; set;} = ""; public int EntityId {get; set;} public int ActorId {get; set;} public DateTimeOffset Timestamp {get; set;} = DateTimeOffset.UtcNow; }

// Minimal DbContext for tests. Replace with your real one then remove this.
public class SmsDbContext : DbContext
{
    public SmsDbContext(DbContextOptions<SmsDbContext> options) : base(options) {}
    public DbSet<Item> Items => Set<Item>();
    public DbSet<Request> Requests => Set<Request>();
    public DbSet<Approval> Approvals => Set<Approval>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
}

// --- Service contracts used in tests ---
public interface IAuditService { void Log(int actorId, string action, string entity, int entityId); }
public interface IInventoryService { void DecrementStock(int itemId, int qty, int actorId); }
public interface IRequestService { Request Create(int itemId, int qty, int requestedById); }
public interface IApprovalService { Approval Approve(int requestId, int approverId); }
public interface IIssueService { void Issue(int requestId, int actorId); }

// --- Simple implementations to exercise business rules ---
// Replace with your real services if available.
public class AuditService : IAuditService
{
    private readonly SmsDbContext _db;
    public AuditService(SmsDbContext db) { _db = db; }
    public void Log(int actorId, string action, string entity, int entityId)
    {
        _db.AuditLogs.Add(new AuditLog{ ActorId = actorId, Action = action, Entity = entity, EntityId = entityId });
        _db.SaveChanges();
    }
}

public class InventoryService : IInventoryService
{
    private readonly SmsDbContext _db;
    private readonly IAuditService _audit;
    public InventoryService(SmsDbContext db, IAuditService audit) { _db = db; _audit = audit; }

    public void DecrementStock(int itemId, int qty, int actorId)
    {
        var item = _db.Items.Find(itemId) ?? throw new InvalidOperationException("Item not found");
        var newQty = item.Stock - qty;
        if (qty <= 0) throw new ArgumentOutOfRangeException(nameof(qty));
        if (newQty < 0) throw new InvalidOperationException("Insufficient stock");
        item.Stock = newQty;
        _db.SaveChanges();
        _audit.Log(actorId, "Issue", "Item", itemId);
    }
}

public class RequestService : IRequestService
{
    private readonly SmsDbContext _db;
    private readonly IAuditService _audit;
    public RequestService(SmsDbContext db, IAuditService audit) { _db = db; _audit = audit; }
    public Request Create(int itemId, int qty, int requestedById)
    {
        if (qty <= 0) throw new ArgumentOutOfRangeException(nameof(qty));
        var req = new Request{ ItemId = itemId, Quantity = qty, RequestedById = requestedById, Status = RequestStatus.Pending };
        _db.Requests.Add(req);
        _db.SaveChanges();
        _audit.Log(requestedById, "CreateRequest", "Request", req.Id);
        return req;
    }
}

public class ApprovalService : IApprovalService
{
    private readonly SmsDbContext _db;
    private readonly IAuditService _audit;
    public ApprovalService(SmsDbContext db, IAuditService audit) { _db = db; _audit = audit; }
    public Approval Approve(int requestId, int approverId)
    {
        var req = _db.Requests.Find(requestId) ?? throw new InvalidOperationException("Request not found");
        if (req.Status != RequestStatus.Pending) throw new InvalidOperationException("Invalid transition");
        req.Status = RequestStatus.Approved;
        var approval = new Approval{ RequestId = requestId, ApprovedById = approverId, Approved = true };
        _db.Approvals.Add(approval);
        _db.SaveChanges();
        _audit.Log(approverId, "Approve", "Request", requestId);
        return approval;
    }
}

public class IssueService : IIssueService
{
    private readonly SmsDbContext _db;
    private readonly IInventoryService _inventory;
    private readonly IAuditService _audit;
    public IssueService(SmsDbContext db, IInventoryService inventory, IAuditService audit) { _db = db; _inventory = inventory; _audit = audit; }
    public void Issue(int requestId, int actorId)
    {
        var req = _db.Requests.Find(requestId) ?? throw new InvalidOperationException("Request not found");
        if (req.Status != RequestStatus.Approved) throw new InvalidOperationException("Request not approved");
        _inventory.DecrementStock(req.ItemId, req.Quantity, actorId);
        req.Status = RequestStatus.Issued;
        _db.SaveChanges();
        _audit.Log(actorId, "IssueRequest", "Request", req.Id);
    }
}

public static class DbUtil
{
    public static SmsDbContext InMemory() {
        var opts = new DbContextOptionsBuilder<SmsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;
        return new SmsDbContext(opts);
    }
}

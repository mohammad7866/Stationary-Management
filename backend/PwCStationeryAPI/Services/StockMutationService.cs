// backend/PwCStationeryAPI/Services/StockMutationService.cs
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PwCStationeryAPI.Data;
using PwCStationeryAPI.Dtos.Items;
using PwCStationeryAPI.Models;

namespace PwCStationeryAPI.Services
{
    public class StockMutationService : IStockMutationService
    {
        private readonly ApplicationDbContext _db;
        private readonly IAuditLogger _audit;
        private readonly IStockAdjustService _adjust;

        public StockMutationService(
            ApplicationDbContext db,
            IAuditLogger audit,
            IStockAdjustService adjust)
        {
            _db = db;
            _audit = audit;
            _adjust = adjust;
        }

        public async Task<Issue> CreateIssueAsync(CreateIssueDto dto, string actorId)
        {
            await using var tx = await _db.Database.BeginTransactionAsync();

            // 1) Load & validate request
            var req = await _db.Requests.FirstOrDefaultAsync(r => r.Id == dto.RequestId)
                      ?? throw new InvalidOperationException("Request not found");

            if (!string.Equals(req.Status, "Approved", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Request is not approved");

            // Enforce one Issue per Request
            if (await _db.Issues.AnyAsync(i => i.RequestId == dto.RequestId))
                throw new InvalidOperationException("Request already issued");

            // 2) Resolve office
            var officeId = await _db.Offices
                .Where(o => o.Name == req.Office)
                .Select(o => o.Id)
                .FirstOrDefaultAsync();
            if (officeId == 0)
                throw new InvalidOperationException($"Office '{req.Office}' not found");

            // 3) Adjust stock (shared logic) — decrement per line
            foreach (var line in dto.Lines)
            {
                if (line.Quantity <= 0)
                    throw new InvalidOperationException("Quantity must be > 0");

                await _adjust.AdjustAsync(line.ItemId, officeId, -line.Quantity,
                    $"Issue for Request {req.Id}");
            }

            // 4) Create Issue + lines
            var issue = new Issue
            {
                RequestId = req.Id,
                IssuedByUserId = actorId,
                IssuedAt = DateTime.UtcNow,
                Lines = dto.Lines.Select(l => new IssueLine
                {
                    ItemId = l.ItemId,
                    Quantity = l.Quantity
                }).ToList()
            };

            _db.Issues.Add(issue);
            await _db.SaveChangesAsync();

            await tx.CommitAsync();
            await _audit.LogAsync(actorId, "IssueCreated", new { dto.RequestId, dto.Lines });

            return issue;
        }

        public async Task<Return> CreateReturnAsync(CreateReturnDto dto, string actorId)
        {
            await using var tx = await _db.Database.BeginTransactionAsync();

            // 1) Load & validate issue
            var issue = await _db.Issues
                .Include(i => i.Lines)
                .FirstOrDefaultAsync(i => i.Id == dto.IssueId)
                ?? throw new InvalidOperationException("Issue not found");

            // 2) Resolve office (from the original request)
            var req = await _db.Requests.FirstOrDefaultAsync(r => r.Id == issue.RequestId)
                      ?? throw new InvalidOperationException("Request not found for issue");

            var officeId = await _db.Offices
                .Where(o => o.Name == req.Office)
                .Select(o => o.Id)
                .FirstOrDefaultAsync();
            if (officeId == 0)
                throw new InvalidOperationException($"Office '{req.Office}' not found");

            // 3) Validate against issued qty, then adjust stock (increment)
            foreach (var line in dto.Lines)
            {
                if (line.Quantity <= 0)
                    throw new InvalidOperationException("Quantity must be > 0");

                var issuedQty = issue.Lines
                    .Where(l => l.ItemId == line.ItemId)
                    .Sum(l => l.Quantity);

                var returnedQtySoFar = await _db.ReturnLines
                    .Where(rl => rl.Return!.IssueId == issue.Id && rl.ItemId == line.ItemId)
                    .SumAsync(rl => (int?)rl.Quantity) ?? 0;

                if (returnedQtySoFar + line.Quantity > issuedQty)
                    throw new InvalidOperationException("Return exceeds issued quantity");

                await _adjust.AdjustAsync(line.ItemId, officeId, +line.Quantity,
                    $"Return for Issue {issue.Id}");
            }

            // 4) Create Return + lines
            var ret = new Return
            {
                IssueId = issue.Id,
                ReturnedByUserId = actorId,
                ReturnedAt = DateTime.UtcNow,
                Lines = dto.Lines.Select(l => new ReturnLine
                {
                    ItemId = l.ItemId,
                    Quantity = l.Quantity
                }).ToList()
            };

            _db.Returns.Add(ret);
            await _db.SaveChangesAsync();

            await tx.CommitAsync();
            await _audit.LogAsync(actorId, "ReturnCreated", new { issue.Id, dto.Lines });

            return ret;
        }
    }
}

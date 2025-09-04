// backend/PwCStationeryAPI/Filters/AuditLogActionFilter.cs
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Filters;
using PwCStationeryAPI.Data;
using PwCStationeryAPI.Models;

namespace PwCStationeryAPI.Filters
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public sealed class SkipAuditAttribute : Attribute { }

    public sealed class AuditLogActionFilter : IAsyncActionFilter
    {
        private readonly ApplicationDbContext _db;
        public AuditLogActionFilter(ApplicationDbContext db) => _db = db;

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.ActionDescriptor.EndpointMetadata.OfType<SkipAuditAttribute>().Any())
            {
                await next();
                return;
            }

            var http = context.HttpContext;
            var sw = System.Diagnostics.Stopwatch.StartNew();

            var user = http.User;
            string? userId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
            string? userName = user.Identity?.Name ?? user.FindFirstValue(ClaimTypes.Name);
            var rolesCsv = string.Join(",", user.FindAll(ClaimTypes.Role).Select(r => r.Value));

            var routeVals = JsonSerializer.Serialize(context.RouteData.Values);
            var query = http.Request?.QueryString.HasValue == true ? http.Request.QueryString.Value : null;

            await next();
            sw.Stop();

            try
            {
                var controller = context.ActionDescriptor.RouteValues.TryGetValue("controller", out var c) ? c : null;
                var actionName = context.ActionDescriptor.RouteValues.TryGetValue("action", out var a) ? a : null;

                var ip = http.Connection?.RemoteIpAddress?.ToString();

                var log = new AuditLog
                {
                    // timestamps
                    TimestampUtc = DateTime.UtcNow,
                    WhenUtc = DateTime.UtcNow,

                    // who
                    UserId = userId,
                    UserName = userName,
                    RolesCsv = rolesCsv,

                    // request
                    Method = http.Request?.Method ?? "",
                    Path = http.Request?.Path.Value ?? "",
                    Controller = controller,
                    Action = actionName,
                    RouteValuesJson = routeVals,
                    QueryString = query,

                    StatusCode = http.Response?.StatusCode ?? 0,
                    DurationMs = (int)sw.ElapsedMilliseconds,

                    Ip = ip,
                    ClientIp = ip, // keep both in sync for your service expectations
                    UserAgent = http.Request?.Headers.UserAgent.ToString(),

                    // change-level fields left null (these are for Services/AuditLogger calls)
                    Entity = null,
                    EntityId = null,
                    BeforeJson = null,
                    AfterJson = null
                };

                _db.AuditLogs.Add(log);
                await _db.SaveChangesAsync();
            }
            catch
            {
                // never break user requests because of logging
            }
        }
    }
}

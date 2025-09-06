using System.Threading.Tasks;
namespace PwCStationeryAPI.Services
{
    public interface IAuditLogger
    {
        Task LogAsync(string actorUserId, string action, object payload);
    }

    // Minimal stub (replace with your real implementation)
    public class NoopAuditLogger : IAuditLogger
    {
        public Task LogAsync(string actorUserId, string action, object payload) => Task.CompletedTask;
    }
}

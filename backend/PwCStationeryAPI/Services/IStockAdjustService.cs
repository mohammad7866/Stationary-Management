using System.Threading;
using System.Threading.Tasks;

namespace PwCStationeryAPI.Services
{
    public interface IStockAdjustService
    {
        /// Adjust stock for (itemId, officeId) by delta. Delta may be negative.
        Task AdjustAsync(int itemId, int officeId, int delta, string reason, CancellationToken ct = default);
    }
}

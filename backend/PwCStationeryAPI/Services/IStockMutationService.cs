using System.Threading.Tasks;
using PwCStationeryAPI.Dtos.Items;
using PwCStationeryAPI.Models;

namespace PwCStationeryAPI.Services
{
    public interface IStockMutationService
    {
        Task<Issue> CreateIssueAsync(CreateIssueDto dto, string actorId);
        Task<Return> CreateReturnAsync(CreateReturnDto dto, string actorId);
    }
}

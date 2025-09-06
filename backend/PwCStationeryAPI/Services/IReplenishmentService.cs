using PwCStationeryAPI.Dtos.Replenishment;


namespace PwCStationeryAPI.Services
{
    public interface IReplenishmentService
    {
        Task<List<LowStockSuggestionDto>> GetSuggestionsAsync(string? office = null, int? minShortage = null);
        Task<int> RaiseDeliveriesAsync(RaiseDeliveriesDto dto, string actorId);
    }
}
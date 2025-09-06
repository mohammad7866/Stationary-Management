using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PwCStationeryAPI.Dtos.Replenishment;
using PwCStationeryAPI.Services;
using System.Security.Claims;


namespace PwCStationeryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,SuperAdmin")] // managers can be added if you prefer
    public class ReplenishmentController : ControllerBase
    {
        private readonly IReplenishmentService _svc;
        public ReplenishmentController(IReplenishmentService svc) { _svc = svc; }


        [HttpGet("suggestions")]
        public async Task<IActionResult> Suggestions([FromQuery] string? office = null, [FromQuery] int? minShortage = null)
        {
            var data = await _svc.GetSuggestionsAsync(office, minShortage);
            return Ok(data);
        }


        [HttpPost("raise")]
        public async Task<IActionResult> Raise([FromBody] RaiseDeliveriesDto dto)
        {
            var actorId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";
            var created = await _svc.RaiseDeliveriesAsync(dto, actorId);
            return Ok(new { created });
        }
    }
}
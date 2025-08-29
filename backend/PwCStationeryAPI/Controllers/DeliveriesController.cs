using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PwCStationeryAPI.Data;
using PwCStationeryAPI.Dtos.Deliveries;
using PwCStationeryAPI.Models;
using Microsoft.AspNetCore.Authorization;


namespace PwCStationeryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeliveriesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public DeliveriesController(ApplicationDbContext db) => _db = db;

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReadDeliveryDto>>> GetAll(
            [FromQuery] string? status = null,
            [FromQuery] string? office = null,
            [FromQuery] string? product = null)
        {
            var q = _db.Deliveries.AsNoTracking()
                .Include(d => d.Supplier)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status)) q = q.Where(d => d.Status == status);
            if (!string.IsNullOrWhiteSpace(office)) q = q.Where(d => d.Office == office);
            if (!string.IsNullOrWhiteSpace(product)) q = q.Where(d => d.Product == product);

            var list = await q
                .OrderBy(d => d.OrderedDateUtc)
                .Select(d => new ReadDeliveryDto
                {
                    Id = d.Id,
                    Product = d.Product,
                    Office = d.Office,
                    SupplierId = d.SupplierId,
                    SupplierName = d.Supplier != null ? d.Supplier.Name : null,

                    OrderedDateUtc = d.OrderedDateUtc,
                    ExpectedArrivalDateUtc = d.ExpectedArrivalDateUtc,
                    ActualArrivalDateUtc = d.ActualArrivalDateUtc,

                    ArrivalDelayDays = d.FinalDelayDays ?? d.ComputedDelayDays,
                    Status = d.Status
                })
                .ToListAsync();

            return Ok(list);
        }

        [Authorize]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ReadDeliveryDto>> GetOne(int id)
        {
            var dto = await _db.Deliveries.AsNoTracking()
                .Include(d => d.Supplier)
                .Where(d => d.Id == id)
                .Select(d => new ReadDeliveryDto
                {
                    Id = d.Id,
                    Product = d.Product,
                    Office = d.Office,
                    SupplierId = d.SupplierId,
                    SupplierName = d.Supplier != null ? d.Supplier.Name : null,

                    OrderedDateUtc = d.OrderedDateUtc,
                    ExpectedArrivalDateUtc = d.ExpectedArrivalDateUtc,
                    ActualArrivalDateUtc = d.ActualArrivalDateUtc,

                    ArrivalDelayDays = d.FinalDelayDays ?? d.ComputedDelayDays,
                    Status = d.Status
                })
                .FirstOrDefaultAsync();

            return dto is null ? NotFound() : Ok(dto);
        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateDeliveryDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var status = string.IsNullOrWhiteSpace(dto.Status) ? "Pending" : dto.Status;

            // Validation
            if (dto.SupplierId.HasValue && !await _db.Suppliers.AnyAsync(s => s.Id == dto.SupplierId.Value))
                return BadRequest($"SupplierId {dto.SupplierId} does not exist.");

            if (dto.ExpectedArrivalDateUtc < dto.OrderedDateUtc)
                return BadRequest("Expected arrival cannot be before the order date.");

            if (dto.ActualArrivalDateUtc.HasValue && status != "Received")
                return BadRequest("ActualArrivalDateUtc can only be set when status is 'Received'.");

            if (status == "Received" && !dto.ActualArrivalDateUtc.HasValue)
                return BadRequest("When status is 'Received', ActualArrivalDateUtc is required.");

            if (dto.ActualArrivalDateUtc.HasValue && dto.ActualArrivalDateUtc.Value.Date < dto.ExpectedArrivalDateUtc.Date)
                return BadRequest("Actual arrival cannot be before expected arrival.");

            var entity = new Delivery
            {
                Product = dto.Product,
                Office = dto.Office,
                SupplierId = dto.SupplierId,

                OrderedDateUtc = dto.OrderedDateUtc,
                ExpectedArrivalDateUtc = dto.ExpectedArrivalDateUtc,
                ActualArrivalDateUtc = dto.ActualArrivalDateUtc,

                Status = status
            };

            // Freeze delay if already Received with actual
            if (status == "Received" && entity.ExpectedArrivalDateUtc.HasValue && entity.ActualArrivalDateUtc.HasValue)
                entity.FinalDelayDays = (int)(entity.ActualArrivalDateUtc.Value.Date - entity.ExpectedArrivalDateUtc.Value.Date).TotalDays;

            _db.Deliveries.Add(entity);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetOne), new { id = entity.Id }, new { id = entity.Id });
        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateDeliveryDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var entity = await _db.Deliveries.FirstOrDefaultAsync(d => d.Id == id);
            if (entity is null) return NotFound();

            var newStatus = dto.Status;

            if (dto.SupplierId.HasValue)
            {
                if (!await _db.Suppliers.AnyAsync(s => s.Id == dto.SupplierId.Value))
                    return BadRequest($"SupplierId {dto.SupplierId} does not exist.");
                entity.SupplierId = dto.SupplierId;
            }

            if (dto.OrderedDateUtc.HasValue)
                entity.OrderedDateUtc = dto.OrderedDateUtc.Value;

            if (dto.ExpectedArrivalDateUtc.HasValue)
            {
                if (dto.ExpectedArrivalDateUtc.Value.Date < entity.OrderedDateUtc.Date)
                    return BadRequest("Expected arrival cannot be before the order date.");
                entity.ExpectedArrivalDateUtc = dto.ExpectedArrivalDateUtc.Value;
            }

            // Actual allowed only with Received
            if (dto.ActualArrivalDateUtc.HasValue && newStatus != "Received")
                return BadRequest("ActualArrivalDateUtc can only be set when status is 'Received'.");

            if (newStatus == "Received" && !dto.ActualArrivalDateUtc.HasValue && !entity.ActualArrivalDateUtc.HasValue)
                return BadRequest("When status is 'Received', ActualArrivalDateUtc is required.");

            if (dto.ActualArrivalDateUtc.HasValue && entity.ExpectedArrivalDateUtc.HasValue &&
                dto.ActualArrivalDateUtc.Value.Date < entity.ExpectedArrivalDateUtc.Value.Date)
                return BadRequest("Actual arrival cannot be before expected arrival.");

            if (dto.ActualArrivalDateUtc.HasValue)
                entity.ActualArrivalDateUtc = dto.ActualArrivalDateUtc.Value;

            // Freeze delay when becoming Received or when actual changes under Received
            var becameReceived = entity.Status != "Received" && newStatus == "Received";
            var actualChangedWhileReceived = newStatus == "Received" && dto.ActualArrivalDateUtc.HasValue;

            if ((becameReceived || actualChangedWhileReceived) &&
                entity.ExpectedArrivalDateUtc.HasValue && entity.ActualArrivalDateUtc.HasValue)
            {
                entity.FinalDelayDays = (int)(entity.ActualArrivalDateUtc.Value.Date - entity.ExpectedArrivalDateUtc.Value.Date).TotalDays;
            }

            entity.Status = newStatus;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpPost("{id:int}/status")]
        public async Task<IActionResult> SetStatus(int id, [FromBody] UpdateDeliveryStatusDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var entity = await _db.Deliveries.FirstOrDefaultAsync(d => d.Id == id);
            if (entity is null) return NotFound();

            if (dto.Status == "Received")
            {
                if (!entity.ActualArrivalDateUtc.HasValue)
                    return BadRequest("When status is 'Received', ActualArrivalDateUtc must be supplied via PUT.");
                if (entity.ExpectedArrivalDateUtc.HasValue && entity.FinalDelayDays == null)
                {
                    entity.FinalDelayDays = (int)(entity.ActualArrivalDateUtc.Value.Date - entity.ExpectedArrivalDateUtc.Value.Date).TotalDays;
                }
            }

            entity.Status = dto.Status;
            await _db.SaveChangesAsync();
            return Ok(new { id = entity.Id, status = entity.Status });
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _db.Deliveries.FindAsync(id);
            if (entity is null) return NotFound();

            _db.Deliveries.Remove(entity);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}

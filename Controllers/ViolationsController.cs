using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TrafficViolationsAPI.Data;
using TrafficViolationsAPI.DTOs;
using TrafficViolationsAPI.Models;

namespace TrafficViolationsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
   // [Authorize]
    public class ViolationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ViolationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: api/violations - تسجيل مخالفة جديدة
        [HttpPost]
        [AllowAnonymous]
        // [Authorize(Policy = "TrafficOfficerOnly")]
        public async Task<ActionResult<ViolationDto>> CreateViolation(CreateViolationDto createViolationDto)
        {
            try
            {
                // Verify vehicle exists
                var vehicle = await _context.Vehicles.FindAsync(createViolationDto.VehicleId);
                if (vehicle == null)
                {
                    return BadRequest(new { message = "المركبة غير موجودة" });
                }

                // Verify officer exists
                var officer = await _context.Users.FindAsync(createViolationDto.OfficerId);
                if (officer == null || officer.UserType != "TrafficOfficer")
                {
                    return BadRequest(new { message = "رجل المرور غير موجود أو غير مخول" });
                }

                var violation = new Violation
                {
                    Type = createViolationDto.Type,
                    Description = createViolationDto.Description,
                    Location = createViolationDto.Location,
                    Timestamp = createViolationDto.Timestamp,
                    FineAmount = createViolationDto.FineAmount,
                    Status = "Pending",
                    VehicleId = createViolationDto.VehicleId,
                    OfficerId = createViolationDto.OfficerId,
                    EvidenceImageUrl = createViolationDto.EvidenceImageUrl,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Violations.Add(violation);
                await _context.SaveChangesAsync();

                // Load related data for response
                await _context.Entry(violation)
                    .Reference(v => v.Vehicle)
                    .LoadAsync();
                await _context.Entry(violation)
                    .Reference(v => v.Officer)
                    .LoadAsync();

                var violationDto = MapToViolationDto(violation);

                return CreatedAtAction(nameof(GetViolation), new { id = violation.Id }, violationDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ أثناء تسجيل المخالفة", error = ex.Message });
            }
        }

        // GET: api/violations/{id} - جلب مخالفة محددة
        [HttpGet("{id}")]
        public async Task<ActionResult<ViolationDto>> GetViolation(Guid id)
        {
            try
            {
                var violation = await _context.Violations
                    .Include(v => v.Vehicle)
                        .ThenInclude(ve => ve.Owner)
                    .Include(v => v.Officer)
                    .FirstOrDefaultAsync(v => v.Id == id);

                if (violation == null)
                {
                    return NotFound(new { message = "المخالفة غير موجودة" });
                }

                var violationDto = MapToViolationDto(violation);
                return Ok(violationDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ أثناء جلب المخالفة", error = ex.Message });
            }
        }

        // GET: api/violations/citizen/{citizenId} - جلب مخالفات مواطن
        [HttpGet("citizen/{citizenId}")]


        public async Task<ActionResult<List<ViolationDto>>> GetCitizenViolations(Guid citizenId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var currentUserType = GetCurrentUserType();

                // Allow access if user is requesting their own violations or if user is traffic officer/admin
                if (currentUserId != citizenId && currentUserType != "TrafficOfficer" && currentUserType != "Admin")
                {
                    return Forbid("غير مخول للوصول لهذه البيانات");
                }

                var violations = await _context.Violations
                    .Include(v => v.Vehicle)
                        .ThenInclude(ve => ve.Owner)
                    .Include(v => v.Officer)
                    .Where(v => v.Vehicle.OwnerId == citizenId)
                    .OrderByDescending(v => v.Timestamp)
                    .ToListAsync();

                var violationDtos = violations.Select(MapToViolationDto).ToList();
                return Ok(violationDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ أثناء جلب المخالفات", error = ex.Message });
            }
        }

        // GET: api/violations/vehicle/{plateNumber} - جلب مخالفات مركبة
        [HttpGet("vehicle/{plateNumber}")]
        public async Task<ActionResult<List<ViolationDto>>> GetVehicleViolations(string plateNumber)
        {
            try
            {
                var violations = await _context.Violations
                    .Include(v => v.Vehicle)
                        .ThenInclude(ve => ve.Owner)
                    .Include(v => v.Officer)
                    .Where(v => v.Vehicle.PlateNumber == plateNumber)
                    .OrderByDescending(v => v.Timestamp)
                    .ToListAsync();

                var violationDtos = violations.Select(MapToViolationDto).ToList();
                return Ok(violationDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ أثناء جلب مخالفات المركبة", error = ex.Message });
            }
        }

        // PUT: api/violations/{id}/status - تحديث حالة المخالفة
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateViolationStatus(Guid id, UpdateViolationStatusDto updateStatusDto)
        {
            try
            {
                var violation = await _context.Violations.FindAsync(id);
                if (violation == null)
                {
                    return NotFound(new { message = "المخالفة غير موجودة" });
                }

                violation.Status = updateStatusDto.Status;
                violation.UpdatedAt = DateTime.UtcNow;

                if (updateStatusDto.Status == "Paid")
                {
                    violation.PaymentDate = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                return Ok(new { message = "تم تحديث حالة المخالفة بنجاح" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ أثناء تحديث حالة المخالفة", error = ex.Message });
            }
        }

        // GET: api/violations - جلب جميع المخالفات (للمشرفين ورجال المرور)
        [HttpGet]
        //[Authorize(Policy = "TrafficOfficerOrAdmin")]
        [AllowAnonymous]
        public async Task<ActionResult<List<ViolationDto>>> GetAllViolations(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? status = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var query = _context.Violations
                    .Include(v => v.Vehicle)
                        .ThenInclude(ve => ve.Owner)
                    .Include(v => v.Officer)
                    .AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(v => v.Status == status);
                }

                if (fromDate.HasValue)
                {
                    query = query.Where(v => v.Timestamp >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    query = query.Where(v => v.Timestamp <= toDate.Value);
                }

                // Apply pagination
                var violations = await query
                    .OrderByDescending(v => v.Timestamp)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var violationDtos = violations.Select(MapToViolationDto).ToList();
                return Ok(violationDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ أثناء جلب المخالفات", error = ex.Message });
            }
        }

        private ViolationDto MapToViolationDto(Violation violation)
        {
            return new ViolationDto
            {
                Id = violation.Id,
                Type = violation.Type,
                Description = violation.Description,
                Location = violation.Location,
                Timestamp = violation.Timestamp,
                FineAmount = violation.FineAmount,
                Status = violation.Status,
                IsPaid = violation.IsPaid,
                PaymentDate = violation.PaymentDate,
                VehicleId = violation.VehicleId,
                OfficerId = violation.OfficerId,
                EvidenceImageUrl = violation.EvidenceImageUrl,
                CreatedAt = violation.CreatedAt,
                Vehicle = violation.Vehicle != null ? new VehicleDto
                {
                    Id = violation.Vehicle.Id,
                    Make = violation.Vehicle.Make,
                    Model = violation.Vehicle.Model,
                    Year = violation.Vehicle.Year,
                    Color = violation.Vehicle.Color,
                    PlateNumber = violation.Vehicle.PlateNumber,
                    VehicleType = violation.Vehicle.VehicleType,
                    OwnerId = violation.Vehicle.OwnerId,
                    RegistrationDate = violation.Vehicle.RegistrationDate,
                    IsActive = violation.Vehicle.IsActive,
                    CreatedAt = violation.Vehicle.CreatedAt,
                    Owner = violation.Vehicle.Owner != null ? new UserDto
                    {
                        Id = violation.Vehicle.Owner.Id,
                        Name = violation.Vehicle.Owner.Name,
                        NationalId = violation.Vehicle.Owner.NationalId,
                        Email = violation.Vehicle.Owner.Email,
                        PhoneNumber = violation.Vehicle.Owner.PhoneNumber,
                        UserType = violation.Vehicle.Owner.UserType,
                        IsActive = violation.Vehicle.Owner.IsActive,
                        CreatedAt = violation.Vehicle.Owner.CreatedAt
                    } : null
                } : null,
                Officer = violation.Officer != null ? new UserDto
                {
                    Id = violation.Officer.Id,
                    Name = violation.Officer.Name,
                    NationalId = violation.Officer.NationalId,
                    Email = violation.Officer.Email,
                    PhoneNumber = violation.Officer.PhoneNumber,
                    UserType = violation.Officer.UserType,
                    IsActive = violation.Officer.IsActive,
                    CreatedAt = violation.Officer.CreatedAt
                } : null
            };
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }

        private string GetCurrentUserType()
        {
            return User.FindFirst("UserType")?.Value ?? "";
        }
    }
}
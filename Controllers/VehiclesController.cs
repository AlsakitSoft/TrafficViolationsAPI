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
    //[Authorize]
    public class VehiclesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public VehiclesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/vehicles - جلب جميع المركبات
        [HttpGet]
      //  [Authorize(Policy = "TrafficOfficerOrAdmin")]
        public async Task<ActionResult<List<VehicleDto>>> GetVehicles(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? plateNumber = null,
            [FromQuery] string? ownerId = null)
        {
            try
            {
                var query = _context.Vehicles
                    .Include(v => v.Owner)
                    .AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(plateNumber))
                {
                    query = query.Where(v => v.PlateNumber.Contains(plateNumber));
                }

                if (!string.IsNullOrEmpty(ownerId) && Guid.TryParse(ownerId, out var ownerGuid))
                {
                    query = query.Where(v => v.OwnerId == ownerGuid);
                }

                // Apply pagination
                var vehicles = await query
                    .OrderBy(v => v.PlateNumber)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var vehicleDtos = vehicles.Select(MapToVehicleDto).ToList();
                return Ok(vehicleDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ أثناء جلب المركبات", error = ex.Message });
            }
        }

        // GET: api/vehicles/{id} - جلب مركبة محددة
        [HttpGet("{id}")]
        public async Task<ActionResult<VehicleDto>> GetVehicle(Guid id)
        {
            try
            {
                var vehicle = await _context.Vehicles
                    .Include(v => v.Owner)
                    .FirstOrDefaultAsync(v => v.Id == id);

                if (vehicle == null)
                {
                    return NotFound(new { message = "المركبة غير موجودة" });
                }

                var currentUserId = GetCurrentUserId();
                var currentUserType = GetCurrentUserType();

                // Allow access if user owns the vehicle or is traffic officer/admin
                if (vehicle.OwnerId != currentUserId && currentUserType != "TrafficOfficer" && currentUserType != "Admin")
                {
                    //return Forbid("غير مخول للوصول لهذه البيانات");
                    return Forbid("Bearer");

                }

                var vehicleDto = MapToVehicleDto(vehicle);
                return Ok(vehicleDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ أثناء جلب المركبة", error = ex.Message });
            }
        }

        // GET: api/vehicles/user/{userId} - جلب مركبات مستخدم
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<VehicleDto>>> GetUserVehicles(Guid userId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var currentUserType = GetCurrentUserType();

                // Allow access if user is requesting their own vehicles or if user is traffic officer/admin
                if (currentUserId != userId && currentUserType != "TrafficOfficer" && currentUserType != "Admin")
                {
                    // return Forbid("غير مخول للوصول لهذه البيانات");
                    return Forbid("Bearer");

                }

                var vehicles = await _context.Vehicles
                    .Include(v => v.Owner)
                    .Where(v => v.OwnerId == userId && v.IsActive)
                    .OrderBy(v => v.PlateNumber)
                    .ToListAsync();

                var vehicleDtos = vehicles.Select(MapToVehicleDto).ToList();
                return Ok(vehicleDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ أثناء جلب مركبات المستخدم", error = ex.Message });
            }
        }

        // GET: api/vehicles/plate/{plateNumber} - البحث بواسطة رقم اللوحة
        [HttpGet("plate/{plateNumber}")]
        public async Task<ActionResult<VehicleDto>> GetVehicleByPlateNumber(string plateNumber)
        {
            try
            {
                var vehicle = await _context.Vehicles
                    .Include(v => v.Owner)
                    .FirstOrDefaultAsync(v => v.PlateNumber == plateNumber);

                if (vehicle == null)
                {
                    return NotFound(new { message = "المركبة غير موجودة" });
                }

                var vehicleDto = MapToVehicleDto(vehicle);
                return Ok(vehicleDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ أثناء البحث عن المركبة", error = ex.Message });
            }
        }

        // POST: api/vehicles - إضافة مركبة جديدة
        [HttpPost]
        public async Task<ActionResult<VehicleDto>> CreateVehicle(CreateVehicleDto createVehicleDto)
        {
            try
            {
                // Check if plate number already exists
                if (await _context.Vehicles.AnyAsync(v => v.PlateNumber == createVehicleDto.PlateNumber))
                {
                    return BadRequest(new { message = "رقم اللوحة مستخدم بالفعل" });
                }

                // Verify owner exists
                var owner = await _context.Users.FindAsync(createVehicleDto.OwnerId);
                if (owner == null)
                {
                    return BadRequest(new { message = "المالك غير موجود" });
                }

                var vehicle = new Vehicle
                {
                    Make = createVehicleDto.Make,
                    Model = createVehicleDto.Model,
                    Year = createVehicleDto.Year,
                    Color = createVehicleDto.Color,
                    PlateNumber = createVehicleDto.PlateNumber,
                    VehicleType = createVehicleDto.VehicleType,
                    OwnerId = createVehicleDto.OwnerId,
                    RegistrationDate = createVehicleDto.RegistrationDate,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Vehicles.Add(vehicle);
                await _context.SaveChangesAsync();

                // Load owner for response
                await _context.Entry(vehicle)
                    .Reference(v => v.Owner)
                    .LoadAsync();

                var vehicleDto = MapToVehicleDto(vehicle);
                return CreatedAtAction(nameof(GetVehicle), new { id = vehicle.Id }, vehicleDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ أثناء إضافة المركبة", error = ex.Message });
            }
        }

        // PUT: api/vehicles/{id} - تحديث مركبة
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVehicle(Guid id, CreateVehicleDto updateVehicleDto)
        {
            try
            {
                var vehicle = await _context.Vehicles.FindAsync(id);
                if (vehicle == null)
                {
                    return NotFound(new { message = "المركبة غير موجودة" });
                }

                var currentUserId = GetCurrentUserId();
                var currentUserType = GetCurrentUserType();

                // Allow update if user owns the vehicle or is traffic officer/admin
                if (vehicle.OwnerId != currentUserId && currentUserType != "TrafficOfficer" && currentUserType != "Admin")
                {
                    // return Forbid("غير مخول لتحديث هذه المركبة");
                    return Forbid("Bearer");

                }

                // Check if new plate number conflicts with existing vehicles
                if (vehicle.PlateNumber != updateVehicleDto.PlateNumber &&
                    await _context.Vehicles.AnyAsync(v => v.PlateNumber == updateVehicleDto.PlateNumber && v.Id != id))
                {
                    return BadRequest(new { message = "رقم اللوحة مستخدم بالفعل" });
                }

                vehicle.Make = updateVehicleDto.Make;
                vehicle.Model = updateVehicleDto.Model;
                vehicle.Year = updateVehicleDto.Year;
                vehicle.Color = updateVehicleDto.Color;
                vehicle.PlateNumber = updateVehicleDto.PlateNumber;
                vehicle.VehicleType = updateVehicleDto.VehicleType;
                vehicle.RegistrationDate = updateVehicleDto.RegistrationDate;
                vehicle.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { message = "تم تحديث المركبة بنجاح" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ أثناء تحديث المركبة", error = ex.Message });
            }
        }

        // DELETE: api/vehicles/{id} - حذف مركبة (إلغاء تفعيل)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVehicle(Guid id)
        {
            try
            {
                var vehicle = await _context.Vehicles.FindAsync(id);
                if (vehicle == null)
                {
                    return NotFound(new { message = "المركبة غير موجودة" });
                }

                var currentUserId = GetCurrentUserId();
                var currentUserType = GetCurrentUserType();

                // Allow deletion if user owns the vehicle or is traffic officer/admin
                if (vehicle.OwnerId != currentUserId && currentUserType != "TrafficOfficer" && currentUserType != "Admin")
                {
                    //return Forbid("غير مخول لحذف هذه المركبة");
                    return Forbid("Bearer");

                }

                // Soft delete - just mark as inactive
                vehicle.IsActive = false;
                vehicle.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { message = "تم حذف المركبة بنجاح" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ أثناء حذف المركبة", error = ex.Message });
            }
        }

        private VehicleDto MapToVehicleDto(Vehicle vehicle)
        {
            return new VehicleDto
            {
                Id = vehicle.Id,
                Make = vehicle.Make,
                Model = vehicle.Model,
                Year = vehicle.Year,
                Color = vehicle.Color,
                PlateNumber = vehicle.PlateNumber,
                VehicleType = vehicle.VehicleType,
                OwnerId = vehicle.OwnerId,
                RegistrationDate = vehicle.RegistrationDate,
                IsActive = vehicle.IsActive,
                CreatedAt = vehicle.CreatedAt,
                Owner = vehicle.Owner != null ? new UserDto
                {
                    Id = vehicle.Owner.Id,
                    Name = vehicle.Owner.Name,
                    NationalId = vehicle.Owner.NationalId,
                    Email = vehicle.Owner.Email,
                    PhoneNumber = vehicle.Owner.PhoneNumber,
                    UserType = vehicle.Owner.UserType,
                    IsActive = vehicle.Owner.IsActive,
                    CreatedAt = vehicle.Owner.CreatedAt
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
        //private Guid GetCurrentUserId()
        //{
        //    // مؤقتًا: إعادة معرف ثابت لمستخدم وهمي أثناء الاختبار
        //    return Guid.Parse("11111111-1111-1111-1111-111111111111");
        //}

        //private string GetCurrentUserType()
        //{
        //    // مؤقتًا: إعادة نوع مستخدم وهمي
        //    return "Admin"; // أو "TrafficOfficer" حسب ما تريد اختباره
        //}
    }
}


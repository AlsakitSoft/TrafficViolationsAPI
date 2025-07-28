//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using System.Security.Claims;
//using TrafficViolationsAPI.Data;
//using TrafficViolationsAPI.DTOs;
//using TrafficViolationsAPI.Models;

//namespace TrafficViolationsAPI.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//   // [Authorize]
//    public class ViolationsController : ControllerBase
//    {
//        private readonly ApplicationDbContext _context;

//        public ViolationsController(ApplicationDbContext context)
//        {
//            _context = context;
//        }

//        // POST: api/violations - تسجيل مخالفة جديدة
//        [HttpPost]
//        [AllowAnonymous]
//        // [Authorize(Policy = "TrafficOfficerOnly")]
//        public async Task<ActionResult<ViolationDto>> CreateViolation(CreateViolationDto createViolationDto)
//        {
//            try
//            {
//                // Verify vehicle exists
//                var vehicle = await _context.Vehicles.FindAsync(createViolationDto.VehicleId);
//                if (vehicle == null)
//                {
//                    return BadRequest(new { message = "المركبة غير موجودة" });
//                }

//                // Verify officer exists
//                var officer = await _context.Users.FindAsync(createViolationDto.OfficerId);
//                if (officer == null || officer.UserType != "TrafficOfficer")
//                {
//                    return BadRequest(new { message = "رجل المرور غير موجود أو غير مخول" });
//                }

//                var violation = new Violation
//                {
//                    Type = createViolationDto.Type,
//                    Description = createViolationDto.Description,
//                    Location = createViolationDto.Location,
//                    Timestamp = createViolationDto.Timestamp,
//                    FineAmount = createViolationDto.FineAmount,
//                    Status = "Pending",
//                    VehicleId = createViolationDto.VehicleId,
//                    OfficerId = createViolationDto.OfficerId,
//                    EvidenceImageUrl = createViolationDto.EvidenceImageUrl,
//                    CreatedAt = DateTime.UtcNow,
//                    UpdatedAt = DateTime.UtcNow
//                };

//                _context.Violations.Add(violation);
//                await _context.SaveChangesAsync();

//                // Load related data for response
//                await _context.Entry(violation)
//                    .Reference(v => v.Vehicle)
//                    .LoadAsync();
//                await _context.Entry(violation)
//                    .Reference(v => v.Officer)
//                    .LoadAsync();

//                var violationDto = MapToViolationDto(violation);

//                return CreatedAtAction(nameof(GetViolation), new { id = violation.Id }, violationDto);
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new { message = "حدث خطأ أثناء تسجيل المخالفة", error = ex.Message });
//            }
//        }

//        // GET: api/violations/{id} - جلب مخالفة محددة
//        [HttpGet("{id}")]
//        public async Task<ActionResult<ViolationDto>> GetViolation(Guid id)
//        {
//            try
//            {
//                var violation = await _context.Violations
//                    .Include(v => v.Vehicle)
//                        .ThenInclude(ve => ve.Owner)
//                    .Include(v => v.Officer)
//                    .FirstOrDefaultAsync(v => v.Id == id);

//                if (violation == null)
//                {
//                    return NotFound(new { message = "المخالفة غير موجودة" });
//                }

//                var violationDto = MapToViolationDto(violation);
//                return Ok(violationDto);
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new { message = "حدث خطأ أثناء جلب المخالفة", error = ex.Message });
//            }
//        }

//        // GET: api/violations/citizen/{citizenId} - جلب مخالفات مواطن
//        [HttpGet("citizen/{citizenId}")]


//        public async Task<ActionResult<List<ViolationDto>>> GetCitizenViolations(Guid citizenId)
//        {
//            try
//            {
//                var currentUserId = GetCurrentUserId();
//                var currentUserType = GetCurrentUserType();

//                // Allow access if user is requesting their own violations or if user is traffic officer/admin
//                if (currentUserId != citizenId && currentUserType != "TrafficOfficer" && currentUserType != "Admin")
//                {
//                    // return Forbid("غير مخول للوصول لهذه البيانات");
//                    return Forbid("Bearer");

//                }

//                var violations = await _context.Violations
//                    .Include(v => v.Vehicle)
//                        .ThenInclude(ve => ve.Owner)
//                    .Include(v => v.Officer)
//                    .Where(v => v.Vehicle.OwnerId == citizenId)
//                    .OrderByDescending(v => v.Timestamp)
//                    .ToListAsync();

//                var violationDtos = violations.Select(MapToViolationDto).ToList();
//                return Ok(violationDtos);
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new { message = "حدث خطأ أثناء جلب المخالفات", error = ex.Message });
//            }
//        }

//        // GET: api/violations/vehicle/{plateNumber} - جلب مخالفات مركبة
//        [HttpGet("vehicle/{plateNumber}")]
//        public async Task<ActionResult<List<ViolationDto>>> GetVehicleViolations(string plateNumber)
//        {
//            try
//            {
//                var violations = await _context.Violations
//                    .Include(v => v.Vehicle)
//                        .ThenInclude(ve => ve.Owner)
//                    .Include(v => v.Officer)
//                    .Where(v => v.Vehicle.PlateNumber == plateNumber)
//                    .OrderByDescending(v => v.Timestamp)
//                    .ToListAsync();

//                var violationDtos = violations.Select(MapToViolationDto).ToList();
//                return Ok(violationDtos);
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new { message = "حدث خطأ أثناء جلب مخالفات المركبة", error = ex.Message });
//            }
//        }

//        // PUT: api/violations/{id}/status - تحديث حالة المخالفة
//        [HttpPut("{id}/status")]
//        public async Task<IActionResult> UpdateViolationStatus(Guid id, UpdateViolationStatusDto updateStatusDto)
//        {
//            try
//            {
//                var violation = await _context.Violations.FindAsync(id);
//                if (violation == null)
//                {
//                    return NotFound(new { message = "المخالفة غير موجودة" });
//                }

//                violation.Status = updateStatusDto.Status;
//                violation.UpdatedAt = DateTime.UtcNow;

//                if (updateStatusDto.Status == "Paid")
//                {
//                    violation.PaymentDate = DateTime.UtcNow;
//                }

//                await _context.SaveChangesAsync();

//                return Ok(new { message = "تم تحديث حالة المخالفة بنجاح" });
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new { message = "حدث خطأ أثناء تحديث حالة المخالفة", error = ex.Message });
//            }
//        }

//        // GET: api/violations - جلب جميع المخالفات (للمشرفين ورجال المرور)
//        [HttpGet]
//        //[Authorize(Policy = "TrafficOfficerOrAdmin")]
//        [AllowAnonymous]
//        public async Task<ActionResult<List<ViolationDto>>> GetAllViolations(
//            [FromQuery] int page = 1,
//            [FromQuery] int pageSize = 10,
//            [FromQuery] string? status = null,
//            [FromQuery] DateTime? fromDate = null,
//            [FromQuery] DateTime? toDate = null)
//        {
//            try
//            {
//                var query = _context.Violations
//                    .Include(v => v.Vehicle)
//                        .ThenInclude(ve => ve.Owner)
//                    .Include(v => v.Officer)
//                    .AsQueryable();

//                // Apply filters
//                if (!string.IsNullOrEmpty(status))
//                {
//                    query = query.Where(v => v.Status == status);
//                }

//                if (fromDate.HasValue)
//                {
//                    query = query.Where(v => v.Timestamp >= fromDate.Value);
//                }

//                if (toDate.HasValue)
//                {
//                    query = query.Where(v => v.Timestamp <= toDate.Value);
//                }

//                // Apply pagination
//                var violations = await query
//                    .OrderByDescending(v => v.Timestamp)
//                    .Skip((page - 1) * pageSize)
//                    .Take(pageSize)
//                    .ToListAsync();

//                var violationDtos = violations.Select(MapToViolationDto).ToList();
//                return Ok(violationDtos);
//            }
//            catch (Exception ex)
//            {
//                return StatusCode(500, new { message = "حدث خطأ أثناء جلب المخالفات", error = ex.Message });
//            }
//        }

//        private ViolationDto MapToViolationDto(Violation violation)
//        {
//            return new ViolationDto
//            {
//                Id = violation.Id,
//                Type = violation.Type,
//                Description = violation.Description,
//                Location = violation.Location,
//                Timestamp = violation.Timestamp,
//                FineAmount = violation.FineAmount,
//                Status = violation.Status,
//                IsPaid = violation.IsPaid,
//                PaymentDate = violation.PaymentDate,
//                VehicleId = violation.VehicleId,
//                OfficerId = violation.OfficerId,
//                EvidenceImageUrl = violation.EvidenceImageUrl,
//                CreatedAt = violation.CreatedAt,
//                Vehicle = violation.Vehicle != null ? new VehicleDto
//                {
//                    Id = violation.Vehicle.Id,
//                    Make = violation.Vehicle.Make,
//                    Model = violation.Vehicle.Model,
//                    Year = violation.Vehicle.Year,
//                    Color = violation.Vehicle.Color,
//                    PlateNumber = violation.Vehicle.PlateNumber,
//                    VehicleType = violation.Vehicle.VehicleType,
//                    OwnerId = violation.Vehicle.OwnerId,
//                    RegistrationDate = violation.Vehicle.RegistrationDate,
//                    IsActive = violation.Vehicle.IsActive,
//                    CreatedAt = violation.Vehicle.CreatedAt,
//                    Owner = violation.Vehicle.Owner != null ? new UserDto
//                    {
//                        Id = violation.Vehicle.Owner.Id,
//                        Name = violation.Vehicle.Owner.Name,
//                        NationalId = violation.Vehicle.Owner.NationalId,
//                        Email = violation.Vehicle.Owner.Email,
//                        PhoneNumber = violation.Vehicle.Owner.PhoneNumber,
//                        UserType = violation.Vehicle.Owner.UserType,
//                        IsActive = violation.Vehicle.Owner.IsActive,
//                        CreatedAt = violation.Vehicle.Owner.CreatedAt
//                    } : null
//                } : null,
//                Officer = violation.Officer != null ? new UserDto
//                {
//                    Id = violation.Officer.Id,
//                    Name = violation.Officer.Name,
//                    NationalId = violation.Officer.NationalId,
//                    Email = violation.Officer.Email,
//                    PhoneNumber = violation.Officer.PhoneNumber,
//                    UserType = violation.Officer.UserType,
//                    IsActive = violation.Officer.IsActive,
//                    CreatedAt = violation.Officer.CreatedAt
//                } : null
//            };
//        }

//        //private Guid GetCurrentUserId()
//        //{
//        //    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
//        //    return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
//        //}

//        //private string GetCurrentUserType()
//        //{
//        //    return User.FindFirst("UserType")?.Value ?? "";
//        //}
//        private Guid GetCurrentUserId()
//        {
//            // مؤقتًا: إعادة معرف ثابت لمستخدم وهمي أثناء الاختبار
//            return Guid.Parse("11111111-1111-1111-1111-111111111111");
//        }

//        private string GetCurrentUserType()
//        {
//            // مؤقتًا: إعادة نوع مستخدم وهمي
//            return "Admin"; // أو "TrafficOfficer" حسب ما تريد اختباره
//        }
//    }
//}

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
     [Authorize]
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
        //[Authorize(Policy = "TrafficOfficerOnly")]
        public async Task<ActionResult<ViolationDto>> CreateViolation(CreateViolationDto createViolationDto)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userType = User.FindFirst("UserType")?.Value;

                if (userType != "TrafficOfficer")
                {
                    return Unauthorized(new { message = "رجل المرور غير موجود أو غير مخول" });
                }

                // الآن userId يحتوي على الـ Guid الخاص بالمستخدم الذي قام بتسجيل الدخول
                var officerId = Guid.Parse(userId);
                //var officerId = GetCurrentUserId();
                //var officerType = GetCurrentUserType();
                // Verify officer exists
                //var officer = await _context.Users.FindAsync(createViolationDto.Created_By_User_ID);
                //if (officer == null || officer.UserType != "TrafficOfficer")
                //{
                //    return BadRequest(new { message = "رجل المرور غير موجود أو غير مخول" });
                //}
                //if (officerId == Guid.Empty || officerType != "TrafficOfficer")
                //{
                //    return BadRequest(new { message = "رجل المرور غير موجود أو غير مخول" });
                //}
                var violation = new Violation
                {
                   // Violation_ID =  Guid.NewGuid().ToString(),
        Violation_Note = createViolationDto.Violation_Note,
                    Violation_Location = createViolationDto.Violation_Location,
                    Plate_Number = createViolationDto.Plate_Number,
                    Plate_Type = createViolationDto.Plate_Type,
                    Dividing = createViolationDto.Dividing,
                    Violation_Type_ID = createViolationDto.Violation_Type_ID,
                    // Created_By_User_ID = createViolationDto.Created_By_User_ID,
                    // Created_At = createViolationDto.Created_At,
                   //Created_By_User_ID = GetCurrentUserId(),
                    Created_By_User_ID = officerId,
                   // Created_At = DateTime.UtcNow,
                    Created_At = createViolationDto.Created_At,
                //Notes = createViolationDto.Notes,
                //ImagePath = createViolationDto.ImagePath,
                //IsSynced = createViolationDto.IsSynced
            };

                _context.Violations.Add(violation);
                await _context.SaveChangesAsync();

                // Load related data for response
                await _context.Entry(violation)
                    .Reference(v => v.ViolationType)
                    .LoadAsync();
                await _context.Entry(violation)
                    .Reference(v => v.CreatedByUser)
                    .LoadAsync();

                var violationDto = MapToViolationDto(violation);

                return CreatedAtAction(nameof(GetViolation), new { id = violation.Violation_ID }, violationDto);
            }
           catch (Exception ex)
{
    return StatusCode(500, new
    {
        message = "حدث خطأ أثناء تسجيل المخالفة",
        error = ex.Message,
        inner = ex.InnerException?.Message
    });
}

        }

        // --- ✅ Endpoint جديد: PUT api/violations/{id} ---
        [HttpPut("{id}")]
        [Authorize(Policy = "TrafficOfficerOnly")]
        public async Task<IActionResult> UpdateViolation(string id, UpdateViolationDto updateDto)
        {
            var officerId = GetCurrentUserId();
            var violation = await _context.Violations.FindAsync(id);

            if (violation == null)
            {
                return NotFound(new { message = "المخالفة غير موجودة" });
            }

            // التحقق من الصلاحية: هل هذا الضابط هو من أنشأ المخالفة؟
            if (violation.Created_By_User_ID != officerId)
            {
                return Forbid(); // 403 Forbidden - ليس لديك صلاحية
            }

            // تحديث الحقول
            violation.Violation_Note = updateDto.Violation_Note;
            violation.Violation_Location = updateDto.Violation_Location;
            violation.Plate_Number = updateDto.Plate_Number;
            //violation.Plate_Type = updateDto.Plate_Type;

            violation.Violation_Type_ID = updateDto.Violation_Type_ID;
           // violation.Notes = updateDto.Notes;
            // يمكنك إضافة حقول أخرى هنا

            _context.Entry(violation).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Violations.Any(e => e.Violation_ID == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent(); // 204 No Content - تم التحديث بنجاح
        }
        // --- ✅ Endpoint جديد: DELETE api/violations/{id} ---
        [HttpDelete("{id}")]
        [Authorize(Policy = "TrafficOfficerOrAdmin")] // الضابط أو المشرف يمكنه الحذف
        public async Task<IActionResult> DeleteViolation(string id)
        {
            var officerId = GetCurrentUserId();
            var userType = GetCurrentUserType();
            var violation = await _context.Violations.FindAsync(id);

            if (violation == null)
            {
                // إذا كانت غير موجودة، نعتبر أن الحذف نجح لتجنب مشاكل المزامنة
                return NoContent();
            }

            // التحقق من الصلاحية: هل هو نفس الضابط أو مشرف؟
            if (violation.Created_By_User_ID != officerId && userType != "Admin")
            {
                return Forbid();
            }

            _context.Violations.Remove(violation);
            await _context.SaveChangesAsync();

            return NoContent(); // 204 No Content - تم الحذف بنجاح
        }


        // GET: api/violations/{id} - جلب مخالفة محددة
        [HttpGet("{id}")]
        public async Task<ActionResult<ViolationDto>> GetViolation(string id)
        {
            try
            {
                var violation = await _context.Violations
                    .Include(v => v.ViolationType)
                    .Include(v => v.CreatedByUser)
                    .FirstOrDefaultAsync(v => v.Violation_ID == id);

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
                    return Forbid("Bearer");
                }

                // Get all vehicles owned by the citizen
                var citizenVehicles = await _context.Vehicles
                    .Where(v => v.OwnerId == citizenId)
                    .Select(v => v.PlateNumber)
                    .ToListAsync();

                var violations = await _context.Violations
                    .Include(v => v.ViolationType)
                    .Include(v => v.CreatedByUser)
                    .Where(v => citizenVehicles.Contains(v.Plate_Number))
                    .OrderByDescending(v => v.Created_At)
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
                    .Include(v => v.ViolationType)
                    .Include(v => v.CreatedByUser)
                    .Where(v => v.Plate_Number == plateNumber)
                    .OrderByDescending(v => v.Created_At)
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
        public async Task<IActionResult> UpdateViolationStatus(string id, UpdateViolationStatusDto updateStatusDto)
        {
            try
            {
                var violation = await _context.Violations.FirstOrDefaultAsync(v => v.Violation_ID == id);
                if (violation == null)
                {
                    return NotFound(new { message = "المخالفة غير موجودة" });
                }

                // Update logic remains the same
                // ...

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
            [FromQuery] DateTime? toDate = null,
            [FromQuery] string? plateNumber = null)
        {
            try
            {
                var query = _context.Violations
                    .Include(v => v.ViolationType)
                    .Include(v => v.CreatedByUser)
                    .AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(v => v.Status == status);
                }

                if (fromDate.HasValue)
                {
                    query = query.Where(v => v.Created_At >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    query = query.Where(v => v.Created_At <= toDate.Value);
                }

                if (!string.IsNullOrEmpty(plateNumber))
                {
                    query = query.Where(v => v.Plate_Number == plateNumber);
                }

                // Apply pagination
                var violations = await query
                    .OrderByDescending(v => v.Created_At)
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
                Violation_ID = violation.Violation_ID,
                Violation_Note = violation.Violation_Note,
                Violation_Location = violation.Violation_Location,
                Plate_Number = violation.Plate_Number,
                Plate_Type = violation.Plate_Type,
                Dividing = violation.Dividing,
                Violation_Type_ID = violation.Violation_Type_ID,
                Created_By_User_ID = violation.Created_By_User_ID,
                Created_At = violation.Created_At,
                //Notes = violation.Notes,
                //ImagePath = violation.ImagePath,
                //IsSynced = violation.IsSynced,
                ViolationType = violation.ViolationType != null ? new ViolationTypeDto
                {
                    Violation_Type_ID = violation.ViolationType.Violation_Type_ID,
                    Violation_Description = violation.ViolationType.Violation_Description
                } : null,
                CreatedByUser = violation.CreatedByUser != null ? new UserDto
                {
                    Id = violation.CreatedByUser.Id,
                    Name = violation.CreatedByUser.Name,
                    NationalId = violation.CreatedByUser.NationalId,
                    Email = violation.CreatedByUser.Email,
                    PhoneNumber = violation.CreatedByUser.PhoneNumber,
                    UserType = violation.CreatedByUser.UserType,
                    IsActive = violation.CreatedByUser.IsActive,
                    CreatedAt = violation.CreatedByUser.CreatedAt
                } : null
            };
        }
        // GET: api/violations/officer/{officerId} - جلب مخالفات رجل مرور معين
        [HttpGet("officer/{officerId}")]
        public async Task<ActionResult<List<ViolationDto>>> GetOfficerViolations(Guid officerId)
        {
            try
            {
                // تأكد من أنّ من يطلب البيانات هو إمّا نفس الضابط أو ADMIn
                var currentUserId = GetCurrentUserId();
                var currentUserType = GetCurrentUserType();
                if (currentUserId != officerId
                    && currentUserType != "TrafficOfficer"
                    && currentUserType != "Admin")
                {
                    return Forbid("Bearer");
                }

                var violations = await _context.Violations
                    .Include(v => v.ViolationType)
                    .Include(v => v.CreatedByUser)
                    .Where(v => v.Created_By_User_ID == officerId)
                    .OrderByDescending(v => v.Created_At)
                    .ToListAsync();

                var dtos = violations.Select(MapToViolationDto).ToList();
                return Ok(dtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ أثناء جلب مخالفات الضابط", error = ex.Message });
            }
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



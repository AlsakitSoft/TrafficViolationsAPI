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
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/users - جلب جميع المستخدمين (للمشرفين فقط)
        [HttpGet]
        [Authorize(Policy = "TrafficOfficerOrAdmin")]
        public async Task<ActionResult<List<UserDto>>> GetUsers(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? userType = null,
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                var query = _context.Users.AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(userType))
                {
                    query = query.Where(u => u.UserType == userType);
                }

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(u => u.Name.Contains(searchTerm) || 
                                           u.Email.Contains(searchTerm) || 
                                           u.NationalId.Contains(searchTerm));
                }

                // Apply pagination
                var users = await query
                    .Where(u => u.IsActive)
                    .OrderBy(u => u.Name)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var userDtos = users.Select(MapToUserDto).ToList();
                return Ok(userDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ أثناء جلب المستخدمين", error = ex.Message });
            }
        }

        // GET: api/users/{id} - جلب مستخدم محدد
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(Guid id)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var currentUserType = GetCurrentUserType();

                // Allow access if user is requesting their own data or if user is traffic officer/admin
                if (currentUserId != id && currentUserType != "TrafficOfficer" && currentUserType != "Admin")
                {
                    return Forbid("غير مخول للوصول لهذه البيانات");
                }

                var user = await _context.Users.FindAsync(id);
                if (user == null || !user.IsActive)
                {
                    return NotFound(new { message = "المستخدم غير موجود" });
                }

                var userDto = MapToUserDto(user);
                return Ok(userDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ أثناء جلب المستخدم", error = ex.Message });
            }
        }

        // GET: api/users/profile - جلب بيانات المستخدم الحالي
        [HttpGet("profile")]
        public async Task<ActionResult<UserDto>> GetCurrentUserProfile()
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var user = await _context.Users.FindAsync(currentUserId);

                if (user == null || !user.IsActive)
                {
                    return NotFound(new { message = "المستخدم غير موجود" });
                }

                var userDto = MapToUserDto(user);
                return Ok(userDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ أثناء جلب بيانات المستخدم", error = ex.Message });
            }
        }

        // GET: api/users/national-id/{nationalId} - البحث بواسطة رقم الهوية
        [HttpGet("national-id/{nationalId}")]
        [Authorize(Policy = "TrafficOfficerOrAdmin")]
        public async Task<ActionResult<UserDto>> GetUserByNationalId(string nationalId)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.NationalId == nationalId && u.IsActive);

                if (user == null)
                {
                    return NotFound(new { message = "المستخدم غير موجود" });
                }

                var userDto = MapToUserDto(user);
                return Ok(userDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ أثناء البحث عن المستخدم", error = ex.Message });
            }
        }

        // PUT: api/users/{id} - تحديث بيانات المستخدم
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(Guid id, UpdateUserDto updateUserDto)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var currentUserType = GetCurrentUserType();

                // Allow update if user is updating their own data or if user is admin
                if (currentUserId != id && currentUserType != "Admin")
                {
                    return Forbid("غير مخول لتحديث هذه البيانات");
                }

                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound(new { message = "المستخدم غير موجود" });
                }

                // Check if new email conflicts with existing users
                if (user.Email != updateUserDto.Email &&
                    await _context.Users.AnyAsync(u => u.Email == updateUserDto.Email && u.Id != id))
                {
                    return BadRequest(new { message = "البريد الإلكتروني مستخدم بالفعل" });
                }

                // Check if new national ID conflicts with existing users
                if (user.NationalId != updateUserDto.NationalId &&
                    await _context.Users.AnyAsync(u => u.NationalId == updateUserDto.NationalId && u.Id != id))
                {
                    return BadRequest(new { message = "رقم الهوية الوطنية مستخدم بالفعل" });
                }

                user.Name = updateUserDto.Name;
                user.Email = updateUserDto.Email;
                user.PhoneNumber = updateUserDto.PhoneNumber;
                user.NationalId = updateUserDto.NationalId;
                user.UpdatedAt = DateTime.UtcNow;

                // Only admin can change user type
                if (currentUserType == "Admin" && !string.IsNullOrEmpty(updateUserDto.UserType))
                {
                    user.UserType = updateUserDto.UserType;
                }

                await _context.SaveChangesAsync();

                return Ok(new { message = "تم تحديث بيانات المستخدم بنجاح" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ أثناء تحديث بيانات المستخدم", error = ex.Message });
            }
        }

        // DELETE: api/users/{id} - حذف مستخدم (إلغاء تفعيل)
        [HttpDelete("{id}")]
        [Authorize(Policy = "TrafficOfficerOrAdmin")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound(new { message = "المستخدم غير موجود" });
                }

                // Soft delete - just mark as inactive
                user.IsActive = false;
                user.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new { message = "تم حذف المستخدم بنجاح" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ أثناء حذف المستخدم", error = ex.Message });
            }
        }

        // GET: api/users/statistics - إحصائيات المستخدمين
        [HttpGet("statistics")]
        [Authorize(Policy = "TrafficOfficerOrAdmin")]
        public async Task<ActionResult<object>> GetUserStatistics()
        {
            try
            {
                var totalUsers = await _context.Users.CountAsync(u => u.IsActive);
                var citizenCount = await _context.Users.CountAsync(u => u.UserType == "Citizen" && u.IsActive);
                var officerCount = await _context.Users.CountAsync(u => u.UserType == "TrafficOfficer" && u.IsActive);
                var adminCount = await _context.Users.CountAsync(u => u.UserType == "Admin" && u.IsActive);

                var statistics = new
                {
                    TotalUsers = totalUsers,
                    CitizenCount = citizenCount,
                    OfficerCount = officerCount,
                    AdminCount = adminCount,
                    NewUsersThisMonth = await _context.Users.CountAsync(u => 
                        u.CreatedAt >= DateTime.UtcNow.AddDays(-30) && u.IsActive)
                };

                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ أثناء جلب الإحصائيات", error = ex.Message });
            }
        }

        private UserDto MapToUserDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                NationalId = user.NationalId,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                UserType = user.UserType,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
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

    public class UpdateUserDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string NationalId { get; set; } = string.Empty;
        public string? UserType { get; set; }
    }
}


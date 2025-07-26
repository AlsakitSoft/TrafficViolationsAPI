using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TrafficViolationsAPI.Data;
using TrafficViolationsAPI.DTOs;
using TrafficViolationsAPI.Models;
using BCrypt.Net;

namespace TrafficViolationsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        //[HttpPost("login")]
        //public async Task<ActionResult<LoginResponseDto>> Login(LoginDto loginDto)
        //{
        //    string hashedPassword = BCrypt.Net.BCrypt.HashPassword("123456");
        //    Console.WriteLine(hashedPassword);

        //    try
        //    {
        //        var user = await _context.Users
        //            .FirstOrDefaultAsync(u => u.Email == loginDto.Email && u.IsActive);

        //        if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
        //        {
        //            return Unauthorized(new { message = "البريد الإلكتروني أو كلمة المرور غير صحيحة" });
        //        }

        //        var token = GenerateJwtToken(user);

        //        var userDto = new UserDto
        //        {
        //            Id = user.Id,
        //            Name = user.Name,
        //            NationalId = user.NationalId,
        //            Email = user.Email,
        //            PhoneNumber = user.PhoneNumber,
        //            UserType = user.UserType,
        //            IsActive = user.IsActive,
        //            CreatedAt = user.CreatedAt
        //        };

        //        return Ok(new LoginResponseDto
        //        {
        //            Token = token,
        //            User = userDto
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = "حدث خطأ أثناء تسجيل الدخول", error = ex.Message });
        //    }
        //}
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login(LoginDto loginDto)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == loginDto.Email && u.IsActive);

                // تحقق أولاً أن المستخدم موجود
                if (user == null)
                {
                    Console.WriteLine("❌ المستخدم غير موجود");
                    return Unauthorized(new { message = "البريد الإلكتروني أو كلمة المرور غير صحيحة" });
                }

                // اطبع الهش من قاعدة البيانات لأغراض التحقق
                Console.WriteLine($"Hash from DB: {user.PasswordHash}");
                Console.WriteLine($"Password match: {BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash)}");

                if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
                {
                    return Unauthorized(new { message = "البريد الإلكتروني أو كلمة المرور غير صحيحة" });
                }

                var token = GenerateJwtToken(user);

                var userDto = new UserDto
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

                return Ok(new LoginResponseDto
                {
                    Token = token,
                    User = userDto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ أثناء تسجيل الدخول", error = ex.Message });
            }
        }


        [HttpPost("register")]
        public async Task<ActionResult<LoginResponseDto>> Register(CreateUserDto createUserDto)
        {
            try
            {
                // Check if user already exists
                if (await _context.Users.AnyAsync(u => u.Email == createUserDto.Email))
                {
                    return BadRequest(new { message = "البريد الإلكتروني مستخدم بالفعل" });
                }

                if (await _context.Users.AnyAsync(u => u.NationalId == createUserDto.NationalId))
                {
                    return BadRequest(new { message = "رقم الهوية الوطنية مستخدم بالفعل" });
                }

                // Create new user
                var user = new User
                {
                    Name = createUserDto.Name,
                    NationalId = createUserDto.NationalId,
                    Email = createUserDto.Email,
                    PhoneNumber = createUserDto.PhoneNumber,
                    UserType = createUserDto.UserType,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var token = GenerateJwtToken(user);

                var userDto = new UserDto
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

                return Ok(new LoginResponseDto
                {
                    Token = token,
                    User = userDto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "حدث خطأ أثناء إنشاء الحساب", error = ex.Message });
            }
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"] ?? "YourSecretKeyHere");

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("UserType", user.UserType),
                new Claim("NationalId", user.NationalId)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
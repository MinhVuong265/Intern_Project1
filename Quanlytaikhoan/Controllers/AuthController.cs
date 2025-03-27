using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserManagementApi.Data;
using UserManagementApi.Models;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Quanlytaikhoan.Services;
using Quanlytaikhoan.Models;

namespace UserManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;
        private readonly UserManagementContext _context;
        private readonly IPasswordHasher _passwordHasher;
        public AuthController(IConfiguration configuration, ILogger<AuthController> logger, UserManagementContext context,IPasswordHasher passwordHasher)
        {
            _configuration = configuration;
            _logger = logger;
            _context = context;
            _passwordHasher = passwordHasher;
        }
        [AllowAnonymous]
        [HttpPost("login")]
        [Produces("application/json")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username);
            if (user == null )
            {
                return Unauthorized("Sai username");

            }
            if(!_passwordHasher.VerifyPassword(request.Password, user.Password, user.salt))
            {
                return Unauthorized("sai mat khau");
            }
            var token = GenerateJwtToken(user);
            _logger.LogInformation("User {Username} logged in successfully at {Time}", user.Username, DateTime.Now);
            return Ok(new { Token = token });
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] LoginRequest request )
        {
            //Kiem tra xem ton tai username chua
            if (await _context.Users.AnyAsync(u =>u.Username == request.Username))
            {
                return BadRequest("Username already exists");
            }
            var hashedPassword = _passwordHasher.HassPassword(request.Password, out var salt);
            //Tao user moi
            var user = new User
            {
                Username = request.Username,
                Password = hashedPassword,
                salt = salt
         
            };
            //Them user vao database 
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            //Gán vai trò mặc định "User" (RoleId = 2, theo dữ liệu seed)
            var userRole = new UserRole
            {
                UserId = user.Id,
                RoleId = 2
            };
            _context.UserRoles.Add(userRole);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User {Username} registered successfully at {Time}", user.Username, DateTime.Now);

            //tao token
            var token = GenerateJwtToken(user);
            return Ok(new { Token = token });

        }

        private string GenerateJwtToken(User user)
        {
            // Lấy danh sách vai trò của người dùng
            var roles = _context.UserRoles
                 .Where(ur => ur.UserId == user.Id)
                 .Select(ur => ur.Role)
                 .ToList();
            // Lấy danh sách RoleId
            var roleIds = roles.Select(r => r.Id).ToList();

            // Lấy danh sách quyền từ các vai trò
            var permission = _context.RolePermissions
                .Where(rp => roleIds.Contains(rp.RoleId))
                .Select(rp => rp.Permission.Name)
                .Distinct()
                .ToList();

            //Tạo claim
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
            };
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Name));
            }
            foreach (var permissions in permission)
            {
                claims.Add(new Claim("Permission", permissions));
            }


            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

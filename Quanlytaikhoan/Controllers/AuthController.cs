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
using System.Text.RegularExpressions;

namespace UserManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;
        private readonly AppDbContext _context;

        public AuthController(IConfiguration configuration, ILogger<AuthController> logger, AppDbContext context)
        {
            _configuration = configuration;
            _logger = logger;
            _context = context;
        }
        [AllowAnonymous]
        [HttpPost("login")]
        [Produces("application/json")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            { 
                return Unauthorized("Invalid credentials");
            }
            var token = GenerateJwtToken(user);
            _logger.LogInformation("User {Username} logged in successfully at {Time}", user.Username, DateTime.Now);
            return Ok(new { Token = token });
        }

        [AllowAnonymous]
        [HttpPost("register")]
        [Produces("application/json")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            // Kiểm tra xem user đã tồn tại chưa
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
            if (existingUser != null)
            {
                return BadRequest("Username already exists.");
            }

            if(string.IsNullOrEmpty(request.Username))
            {
                return BadRequest("Please enter username");
            }

            if (string.IsNullOrEmpty(request.Password))
            {
                return BadRequest("Please enter password");
            }

            if (!Regex.IsMatch(request.Username, "^[a-zA-Z0-9]+$"))
            {
                return BadRequest("Username must not contain spaces or special characters.");
            }

            if (request.Password.Contains(" "))
            {
                return BadRequest("Password must not contain spaces.");
            }

            // Hash mật khẩu trước khi lưu
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password, 13);

            // Tạo user mới
            var newUser = new User
            {
                Username = request.Username,
                Password = hashedPassword,
                Role = request.Role
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok("User registered successfully");
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role)
            };

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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserManagementApi.Data;
using UserManagementApi.Models;
using BCrypt.Net;
using System.Security.Claims;
using System.Linq;
using Quanlytaikhoan.Services;
using Quanlytaikhoan.Models;

namespace UserManagementApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly UserManagementContext _dbcontext;
        private readonly IPasswordHasher _passwordHasher;

        public UsersController(UserManagementContext context, IPasswordHasher passwordHasher)
        {
            _dbcontext = context;
            _passwordHasher = passwordHasher;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult GetAll()
        {
            Console.WriteLine($"User role: {User.FindFirst(ClaimTypes.Role)?.Value}");
            var users = _dbcontext.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .ToList();
            if (!users.Any())
            {
                return NotFound("Không có người dùng nào");
            }

            //Su dung Dto
            var userDtos = users.Select(u => new UserDto
            {
                Id = u.Id,
                Username = u.Username,
                UserRoleDtos = u.UserRoles?.Select(ur => new UserRoleDto
                {
                    UserId = ur.UserId,
                    RoleId = ur.RoleId,
                    Role = new RoleDto
                    {
                        Id = ur.RoleId,
                        Name = ur.Role.Name,
                    }
                }).ToList()
            }).ToList();
            return Ok(userDtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _dbcontext.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound($"Không tìm thấy người dùng với ID {id}");

            var IdOfCurrentUser = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (User.HasClaim(c=>c.Type == "Permission" && c.Value == "ViewProfile") || IdOfCurrentUser == id) {
                return user;
            }
            return Forbid();
            
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<User>> CreateUser([FromBody] User user)
        {
            if (!User.HasClaim(c => c.Type == "Permission" && c.Value == "CreateUser"))
            {
                return Forbid();
            }

            user.Password = _passwordHasher.HassPassword(user.Password, out var salt);
            user.salt = salt;

            // thêm user vào db
            _dbcontext.Users.Add(user);
            await _dbcontext.SaveChangesAsync();

            // Gán vai trò mặc định "User"
            var userRole = new UserRole
            {
                UserId = user.Id,
                RoleId = 2

            };
            _dbcontext.UserRoles.Add(userRole);
            await _dbcontext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }
        //Gán vai trò mặc định "User"

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        
       
        public async Task<IActionResult> UpdateUser(int id, [FromBody] User user)
        {
            if (!User.HasClaim(c=>c.Type == "Permission" && c.Value =="EditUser"))
            {
                return Forbid();
            }

            var tontaiUser = await _dbcontext.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (tontaiUser == null)
            {
                return NotFound();
            }

            tontaiUser.Username = user.Username;
            if (!string.IsNullOrEmpty(user.Password))
            {
                tontaiUser.Password = _passwordHasher.HassPassword(user.Password, out var salt);
                tontaiUser.salt = salt;
            }
            

            try {
                await _dbcontext.SaveChangesAsync();
                return NoContent();
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error updating user: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            if (!User.HasClaim(c=>c.Type == "Permission" && c.Value == "DeleteUser"))
            {
                return Forbid();
            }
            var IDnguoidunghientai = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var Rolenguoidunghientai = User.FindFirst(ClaimTypes.Role)?.Value;

            //Lấy thông tin người dùng bị xóa
            var usercanxoa = await _dbcontext.Users.FirstOrDefaultAsync(u => u.Id == id);
            if(IDnguoidunghientai == id)
            {
                return BadRequest("Bạn không thể xóa chính mình");
            }
            if (usercanxoa == null)
            {
                return NotFound("Người dùng không tồn tại");
            }
            // Lấy vai trò người bị xóa
            var userRole = await _dbcontext.UserRoles
                .Where(ur => ur.UserId == usercanxoa.Id)
                .Select(ur => ur.Role.Name)
                .ToListAsync();

            if(Rolenguoidunghientai == "Admin" && userRole.Contains("Admin"))
            {
                return BadRequest("Admin không thể xóa tài khoản Admin khác");
            }

            _dbcontext.Users.Remove(usercanxoa);
            try
            {
                await _dbcontext.SaveChangesAsync();
                return NoContent();
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error deleting user: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
            
        }
    }
}
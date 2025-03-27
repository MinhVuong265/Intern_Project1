using UserManagementApi.Models;

namespace Quanlytaikhoan.Models
{
    public class UserRole
    {
        public int UserId { get; set; }
        public User? user { get; set; }
        public int RoleId { get; set; }
        public Role? Role { get; set; }
    }
}

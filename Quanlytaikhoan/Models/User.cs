using Quanlytaikhoan.Models;

namespace UserManagementApi.Models
{
    public class User
    {
        public int Id { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; } // Lưu dạng hash
        public byte[]? salt { get; set; }
        public List<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
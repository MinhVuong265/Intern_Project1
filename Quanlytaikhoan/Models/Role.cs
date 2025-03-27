namespace Quanlytaikhoan.Models
{
    public class Role
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public List<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
        public List<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}

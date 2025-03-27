namespace Quanlytaikhoan.Models
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public List<UserRoleDto> UserRoleDtos { get; set; }

    }
    public class UserRoleDto
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public RoleDto Role { get; set; }
    }

    public class RoleDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}

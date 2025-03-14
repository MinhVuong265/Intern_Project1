namespace UserManagementApi.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; } // Lưu dạng hash
        public string Role { get; set; }
    }
}
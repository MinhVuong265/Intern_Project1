using Microsoft.EntityFrameworkCore;
using UserManagementApi.Models;

namespace UserManagementApi.Data
{
    public class AppDbContext : DbContext
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Username = "admin", Password = BCrypt.Net.BCrypt.HashPassword("admin123"), Role = "Admin" },
                new User { Id = 2, Username = "user", Password = BCrypt.Net.BCrypt.HashPassword("user123"), Role = "User" }
            );
        }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; } // Bảng Users trong database
    }
}
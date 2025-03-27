using Microsoft.EntityFrameworkCore;
using Quanlytaikhoan.Models;
using UserManagementApi.Models;

namespace UserManagementApi.Data
{
     public class UserManagementContext : DbContext
    {
        public DbSet<User> Users { get; set; } // Bảng Users trong database
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }

        public DbSet<UserRole> UserRoles { get; set; }
        public UserManagementContext(DbContextOptions<UserManagementContext> options)
            : base(options)
        {
            
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Cấu hình khóa chính cho RolePermission
            modelBuilder.Entity<RolePermission>()
                .HasKey(rp => new { rp.RoleId,rp.PermissionId});

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId);

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId);

            //cấu hình khóa chính cho UserRole
            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.user)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<UserRole>()
                .HasOne(r => r.Role)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.RoleId);



            //dữ liệu seed
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Admin"},
                new Role { Id = 2, Name = "User"}
            );
            modelBuilder.Entity<Permission>().HasData(
                new Permission { Id = 1, Name = "CreateUser", Description ="Can Create user"},
                new Permission { Id = 2, Name = "DeleteUser", Description = "Can Delete user"},
                new Permission { Id = 3, Name = "ViewProfile", Description = "Can View_profile"},
                new Permission { Id = 4, Name = "EditUser", Description ="Can edit profile" }
            );
            modelBuilder.Entity<RolePermission>().HasData(
                new RolePermission { RoleId = 1, PermissionId = 1},
                new RolePermission { RoleId = 1, PermissionId = 2 },
                new RolePermission { RoleId = 1, PermissionId = 3 },
                new RolePermission { RoleId = 1, PermissionId = 4 },

                new RolePermission { RoleId = 2, PermissionId = 3 }
            );

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    Password = "050736116B30666D79574E432013F3AE",
                    salt = new byte[]
                    {
                        0x13, 0x39, 0x42, 0x75, 0x42, 0x0A, 0xE0, 0x76, 0xEC, 0x75, 0x18, 0xE6, 0x90, 0xBA, 0xD2, 0xD0
                    }
                },
                new User
                {
                    Id = 2,
                    Username = "user",
                    Password = "050736116B30666D79574E432013F3AE",
                    salt = new byte[]
                    {
                         0x13, 0x39, 0x42, 0x75, 0x42, 0x0A, 0xE0, 0x76, 0xEC, 0x75, 0x18, 0xE6, 0x90, 0xBA, 0xD2, 0xD0
                    }
                }
            );

            modelBuilder.Entity<UserRole>().HasData(
                new UserRole { UserId = 1, RoleId = 1 },// Gán vai trò Admin cho user admin
                new UserRole { UserId = 2, RoleId = 2}
            );
        }
    } 
}
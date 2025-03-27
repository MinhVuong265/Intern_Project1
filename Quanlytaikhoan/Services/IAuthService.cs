using UserManagementApi.Models;

namespace Quanlytaikhoan.Services
{
    public interface IAuthService
    {
        (List<string> roles, List<string> permissions) DecodeToken(string token);
        void SetRoleToSession(ISession session, List<string> roles, List<string> permissions);
        string GetRoleFromSession(ISession session);
        List<string> GetPermissionsFromSession(ISession session);
        Task<(bool success, string errorMessage)> LoginAsync(string username, string password, HttpResponse httpResponse, ISession session);
        Task<(bool success, string errorMessage)> RegisterAsync(string username, string password, HttpResponse httpResponse, ISession session);
        void Logout(HttpResponse response, ISession session);
        void ClearSession(ISession session);
        bool IsAuthenticated(HttpRequest request);
        bool HasPermission(HttpRequest request, string permission);
        Task<(bool success, string errorMessage)> UpdateUser(User user, HttpRequest request);
    }
}

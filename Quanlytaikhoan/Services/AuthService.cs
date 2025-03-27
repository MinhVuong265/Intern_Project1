
using Microsoft.AspNetCore.Http;
using Quanlytaikhoan.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using UserManagementApi.Models;

namespace Giaodien.Services
{
    public class AuthService : IAuthService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string baseUrl = "https://localhost:7036/api/";
        private const string RoleSessionKey = "UserRole";
        private const string PermissionsSessionKey = "UserPermissions";

        public AuthService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public bool IsAuthenticated(HttpRequest request)
        {
            return request.Cookies.ContainsKey("JwtToken");
        }

        public bool HasPermission(HttpRequest request, string permission)
        {
            var token = request.Cookies["JwtToken"];
            if (string.IsNullOrEmpty(token))
            {
                return false;
            }

            var (roles, permissions) = DecodeToken(token);
            return permissions.Contains(permission);
        }

        public string GetRoleFromSession(ISession session)
        {
            return session.GetString(RoleSessionKey) ?? "Guest";
        }

        public List<string> GetPermissionsFromSession(ISession session)
        {
            var permissionsJson = session.GetString(PermissionsSessionKey);
            if (string.IsNullOrEmpty(permissionsJson))
            {
                return new List<string>();
            }
            return JsonSerializer.Deserialize<List<string>>(permissionsJson) ?? new List<string>();
        }

        public async Task<(bool success, string errorMessage)> LoginAsync(string username, string password, HttpResponse response, ISession session)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri(baseUrl);

                var loginData = new { username, password };
                var content = new StringContent(JsonSerializer.Serialize(loginData), System.Text.Encoding.UTF8, "application/json");

                var apiResponse = await client.PostAsync("auth/login", content);
                if (apiResponse.IsSuccessStatusCode)
                {
                    var json = await apiResponse.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                    var token = result["token"];

                    response.Cookies.Append("JwtToken", token, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTimeOffset.UtcNow.AddHours(1)
                    });

                    var (roles, permissions) = DecodeToken(token);
                    SetRoleToSession(session, roles, permissions);

                    return (true, null);
                }
                return (false, "Tên người dùng hoặc mật khẩu không đúng.");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi đăng nhập: {ex.Message}");
            }
        }

        public async Task<(bool success, string errorMessage)> RegisterAsync(string username, string password, HttpResponse response, ISession session)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri(baseUrl);

                var registerData = new { username, password };
                var content = new StringContent(JsonSerializer.Serialize(registerData), System.Text.Encoding.UTF8, "application/json");

                var apiResponse = await client.PostAsync("auth/register", content);
                if (apiResponse.IsSuccessStatusCode)
                {
                    var json = await apiResponse.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                    var token = result["token"];

                    response.Cookies.Append("JwtToken", token, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTimeOffset.UtcNow.AddHours(1)
                    });

                    var (roles, permissions) = DecodeToken(token);
                    SetRoleToSession(session, roles, permissions);

                    return (true, null);
                }
                return (false, "Không thể đăng ký người dùng.");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi đăng ký: {ex.Message}");
            }
        }

        public async Task<(bool success, string errorMessage)> UpdateUser(User user, HttpRequest request)
        {
            try
            {
                var token = request.Cookies["JwtToken"];
                if (string.IsNullOrEmpty(token))
                {
                    return (false, "Không tìm thấy token xác thực.");
                }

                var client = _httpClientFactory.CreateClient();
                client.BaseAddress = new Uri(baseUrl);
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var content = new StringContent(JsonSerializer.Serialize(user), System.Text.Encoding.UTF8, "application/json");
                var response = await client.PutAsync($"users/{user.Id}", content);

                if (response.IsSuccessStatusCode)
                {
                    return (true, null);
                }
                return (false, "Không thể cập nhật người dùng.");
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi khi cập nhật người dùng: {ex.Message}");
            }
        }

        public void Logout(HttpResponse response, ISession session)
        {
            response.Cookies.Delete("JwtToken");
            session.Clear();
        }

        public (List<string> roles, List<string> permissions) DecodeToken(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                var roles = jwtToken.Claims
                    .Where(c => c.Type == ClaimTypes.Role)
                    .Select(c => c.Value)
                    .ToList();

                var permissions = jwtToken.Claims
                    .Where(c => c.Type == "Permission")
                    .Select(c => c.Value)
                    .ToList();

                return (roles, permissions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error decoding token: {ex.Message}");
                return (new List<string>(), new List<string>());
            }
        }

        public void SetRoleToSession(ISession session, List<string> roles, List<string> permissions)
        {
            if (roles != null && roles.Any())
            {
                session.SetString(RoleSessionKey, roles.First());
            }
            if (permissions != null && permissions.Any())
            {
                session.SetString(PermissionsSessionKey, JsonSerializer.Serialize(permissions));
            }
        }

        (List<string> roles, List<string> permissions) IAuthService.DecodeToken(string token)
        {
            throw new NotImplementedException();
        }

        void IAuthService.SetRoleToSession(ISession session, List<string> roles, List<string> permissions)
        {
            throw new NotImplementedException();
        }

        string IAuthService.GetRoleFromSession(ISession session)
        {
            throw new NotImplementedException();
        }

        List<string> IAuthService.GetPermissionsFromSession(ISession session)
        {
            throw new NotImplementedException();
        }

        Task<(bool success, string errorMessage)> IAuthService.LoginAsync(string username, string password, HttpResponse httpResponse, ISession session)
        {
            throw new NotImplementedException();
        }

        Task<(bool success, string errorMessage)> IAuthService.RegisterAsync(string username, string password, HttpResponse httpResponse, ISession session)
        {
            throw new NotImplementedException();
        }

        void IAuthService.Logout(HttpResponse response, ISession session)
        {
            throw new NotImplementedException();
        }

        void IAuthService.ClearSession(ISession session)
        {
            throw new NotImplementedException();
        }

        bool IAuthService.IsAuthenticated(HttpRequest request)
        {
            throw new NotImplementedException();
        }

        bool IAuthService.HasPermission(HttpRequest request, string permission)
        {
            throw new NotImplementedException();
        }

        Task<(bool success, string errorMessage)> IAuthService.UpdateUser(User user, HttpRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Quanlytaikhoan.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfigController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ConfigController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("jwt")]
        public IActionResult GetJwtConfig()
        {
            var jwtKey = _configuration["Jwt:Key"];
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];
            return Ok(new { jwtKey, issuer, audience });
        }
    }
}

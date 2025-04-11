using backend_english.Request;
using backend_english.Response;
using Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace backend_english.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService authService;

        public AuthController(IAuthService authService)
        {
            this.authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AuthRequest user)
        {
            var token = await authService.LoginAccount(user.email, user.password);
            if (!string.IsNullOrEmpty(token))
            {
                return Ok(new ApiResponse<AuthResponse>(200, null, new AuthResponse { Token = token, ExpiresAt = DateTime.UtcNow.AddHours(24) }));
            }
            return Unauthorized();
        }

    }
}

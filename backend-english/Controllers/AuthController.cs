using backend_english.Request;
using backend_english.Response;
using Core.Models;
using Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace backend_english.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService authService;
        private readonly IAccountService accountService;

        public AuthController(IAuthService authService, IAccountService accountService)
        {
            this.authService = authService;
            this.accountService = accountService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AuthRequest user)
        {
            try
            {
                var token = await authService.LoginAccount(user.email, user.password);
                if (!string.IsNullOrEmpty(token))
                {
                    return Ok(new ApiResponse<AuthResponse>(200, null, new AuthResponse { Token = token, ExpiresAt = DateTime.UtcNow.AddHours(24) }));
                }
                return Unauthorized();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error with service" + ex.Message);
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAccount([FromBody] AccRegisterRequest acc)
        {
            try
            {
                if (!string.IsNullOrEmpty(acc.fullName) && !string.IsNullOrEmpty(acc.email) && !string.IsNullOrEmpty(acc.plainPassword))
                {
                    var duplicate = await accountService.GetAccountByEmailAsync(acc.email);
                    if (duplicate == null)
                    {
                        var token = await authService.RegisterAccount(acc.fullName, acc.email, acc.plainPassword);
                        if (!string.IsNullOrEmpty(token))
                        {
                            return Ok(new ApiResponse<AuthResponse>(200, null, new AuthResponse { Token = token, ExpiresAt = DateTime.UtcNow.AddHours(24) }));
                        }
                        return Unauthorized();
                    }
                    return BadRequest(new ApiResponse<string>(400, "This email is already existed", null));
                }
                return BadRequest(new ApiResponse<string>(400, "Please check all information", null));
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error with service" + ex.Message);
            }
        }



        [Authorize]
        [HttpGet("currentAccount")]
        public async Task<IActionResult> GetCurrentAccount()
        {
            try
            {
                var email = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (email != null)
                {
                    var curAcc = await accountService.GetAccountByEmailAsync((string)email);
                    if (curAcc != null)
                    {
                        return Ok(new ApiResponse<Account>(200, null, curAcc));
                    }
                }
                return Unauthorized();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error with service" + ex.Message);
            }

        }


    }
}

using backend_english.Response;
using Core.Models;
using Infrastructure.Services.Implements;
using Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace backend_english.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService accountService;
        private readonly IWSService wSService;
        public AccountController(IAccountService accountService, IWSService wSService)
        {
            this.accountService = accountService;
            this.wSService = wSService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllAccount()
        {
            return Ok(new ApiResponse<string>(200, "BE has not been doing anything.HEHE", null));

        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAccountById(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return BadRequest(new ApiResponse<string>(400, "Invalid accounr ID.", null));

                var account = await accountService.GetAccountByIdAsync(id);
                if (account == null)
                    return NotFound(new ApiResponse<string>(404, "Account not found.", null));

                return Ok(new ApiResponse<Account>(200, null, account));
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error with service" + ex.Message);
            }
        }

        [HttpGet("email/{email}")]
        public async Task<IActionResult> GetAccountByEmail(string email)
        {
            try
            {
                email = email.Trim();
                if (string.IsNullOrEmpty(email))
                    return BadRequest(new ApiResponse<string>(400, "Invalid account email.", null));

                var account = await accountService.GetAccountByEmailAsync(email);
                if (account == null)
                    return NotFound(new ApiResponse<string>(404, "Account not found.", null));

                return Ok(new ApiResponse<Account>(200, null, account));
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error with service" + ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddNewAccount([FromBody] Account newAcc)
        {
            if (string.IsNullOrEmpty(newAcc.fullName) || string.IsNullOrEmpty(newAcc.email))
                return BadRequest(new ApiResponse<string>(400, "Invalid Account data.", null));
            try
            {
                var success = await accountService.AddNewAccountAsync(newAcc);
                if (success != null)
                {
                    var defaultWordSet = new WordSet
                    {
                        wordsetId = Guid.NewGuid(),
                        nameSet = "Saved Words",
                        accountId = success.accountId
                    };
                    var wordSetSuccess = await wSService.AddNewWordSetAsync(defaultWordSet);

                    if (wordSetSuccess == null || wordSetSuccess.wordsetId == Guid.Empty)
                    {
                        Console.WriteLine("Failed to create default 'Saved Words' word set.");
                    }

                    return Ok(new ApiResponse<Account>(200, "Success", success));
                }
                else
                {
                    return StatusCode(500, "Failed to add new account.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error with service: {ex.Message}");
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateAccount([FromBody] Account newAcc)
        {
            if (string.IsNullOrEmpty(newAcc.fullName) || string.IsNullOrEmpty(newAcc.email) || newAcc.accountId == Guid.Empty)
                return BadRequest(new ApiResponse<string>(400, "Invalid Account data.", null));
            try
            {
                var success = await accountService.UpdateAccountAsync(newAcc);
                if (success != null)
                {
                    return Ok(new ApiResponse<Account>(200, "Success", success));
                }
                else
                {
                    return StatusCode(500, "Failed to  update account.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error with service: {ex.Message}");
            }
        }
    }
}

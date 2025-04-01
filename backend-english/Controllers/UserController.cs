using Core.Models;
using Infrastructure.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace backend_english.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IAccountRepository accountRepository;
        public UserController(IAccountRepository accountRepository)
        {
            this.accountRepository = accountRepository;
        }
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var acc = await accountRepository.GetAccountByEmail("nhih");
            return Ok(acc);
        }

        [HttpPost]
        public async Task<IActionResult> AddNewUsers()
        {
            Account acc = new Account();
            var newAcc = await accountRepository.AddNewAccount(acc);
            return Ok(newAcc);
        }
    }
}

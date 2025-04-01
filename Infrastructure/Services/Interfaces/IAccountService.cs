using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.Interfaces
{
    public interface IAccountService
    {
        Task<Account?> GetAccountByIdAsync(Guid AccountId);
        Task<Account?> GetAccountByEmailAsync(string email);
        Task<Account?> AddNewAccountAsync(Account newAccount);
        Task<Account> UpdateAccountAsync(Account updatedAccount);
    }
}

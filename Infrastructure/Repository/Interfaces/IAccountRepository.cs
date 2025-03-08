using Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repository
{
    public interface IAccountRepository
    {
        Task<Account> GetAccountById(Guid AccountId);
        Task GetAccountByEmail(string email);
        Task<Account> AddNewAccount(Account newAccount);
        Task<Account> UpdateAccount(Account updatedAccount);
    }
}

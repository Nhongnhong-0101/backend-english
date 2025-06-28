using Core.Models;
using Infrastructure.Repository;
using Infrastructure.Repository.Implements;
using Infrastructure.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.Implements
{
    public class AccountService : IAccountService
    {
        private readonly IAccountRepository accountRepository;

        public AccountService(IAccountRepository accountRepository)
        {
            this.accountRepository = accountRepository;
        }

        public async Task<Account?> AddNewAccountAsync(Account newAccount)
        {
            try
            {
                if (newAccount == null)
                    throw new ArgumentNullException("Account cannot be null");

                if (string.IsNullOrWhiteSpace(newAccount.email) || string.IsNullOrWhiteSpace(newAccount.fullName))
                    throw new ArgumentException("Please check your data");
                var existed = await accountRepository.GetAccountByEmailAsync(newAccount.email);
                if(existed != null && existed.accountId != Guid.Empty)
                {
                    throw new ArgumentException("There is an existed account with this email");
                }
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(newAccount.passwordHash);
                newAccount.passwordHash = hashedPassword;
                return await accountRepository.AddNewAccountAsync(newAccount);

            }
            catch (Exception ex)
            {
                throw new Exception("Failed from service: " + ex.Message);   
            }
        }

        public async Task<Account?> GetAccountByEmailAsync(string email)
        {
            try
            {
                email = email.Trim();
                if (string.IsNullOrWhiteSpace(email))
                    throw new ArgumentException("Please check your data");

                return await accountRepository.GetAccountByEmailAsync(email);

            }
            catch (Exception ex)
            {
                throw new Exception("Failed from service: " + ex.Message);
            }
        }

        public async Task<Account?> GetAccountByIdAsync(Guid AccountId)
        {
            try
            {
                if (AccountId == Guid.Empty)
                    throw new ArgumentException("Please check your data");

                return await accountRepository.GetAccountByIdAsync(AccountId);

            }
            catch (Exception ex)
            {
                throw new Exception("Failed from service: " + ex.Message);
            }
        }

        public async Task<Account> UpdateAccountAsync(Account updatedAccount)
        {
            try
            {
                if (updatedAccount == null)
                    throw new ArgumentNullException("Account cannot be null");

                if (string.IsNullOrWhiteSpace(updatedAccount.email) || string.IsNullOrWhiteSpace(updatedAccount.fullName))
                    throw new ArgumentException("Please check your data");
                var existedAcc = await accountRepository.GetAccountByIdAsync(updatedAccount.accountId);
                if (existedAcc == null)
                {
                    throw new ArgumentException("Your account is not existed");
                }
                else
                {
                    string hashedPassword = BCrypt.Net.BCrypt.HashPassword(updatedAccount.passwordHash);
                    updatedAccount.passwordHash = hashedPassword;
                    return await accountRepository.UpdateAccountAsync(updatedAccount);  
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed from service: " + ex.Message);
            }
        }
    }
}

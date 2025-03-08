using Core;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repository.Implements
{
    public class AccountRepository : IAccountRepository
    {
        private readonly string connectionString;

        public AccountRepository(IConfiguration configuration)
        {
            this.connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        public Task<Account> AddNewAccount(Account newAccount)
        {
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    var sql = "INSERT INTO Account (AccountId, Email, Password, FullName, Avatar, Role, CreatedAt, UpdatedAt) VALUES (@AccountId, @Email, @Password, @FullName, @Avatar, @Role, @CreatedAt, @UpdatedAt)";
                    var result = connection.Execute(sql, newAccount);
                    if (result == 1)
                    {
                        return Task.FromResult(newAccount);
                    }
                    return Task.FromResult<Account>(null);
                }
            }
            catch (Exception ex)
            {
                return Task.FromResult<Account>(null);
            }
        }

        public Task GetAccountByEmail(string email)
        {

            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    var sql = "select * from account where email = '21522423@gm.uit.edu.vn'" ;
                    var result =  connection.Query(sql).ToList();
                    //how to parse result to Account object
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public Task<Account> GetAccountById(Guid AccountId)
        {
            throw new NotImplementedException();
        }

        public Task<Account> UpdateAccount(Account updatedAccount)
        {
            throw new NotImplementedException();
        }
    }
}

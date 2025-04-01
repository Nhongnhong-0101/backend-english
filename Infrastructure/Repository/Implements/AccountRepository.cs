using Core.Models;
using Dapper;
using Infrastructure.Config;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repository.Implements
{
    public class AccountRepository : IAccountRepository
    {
        private readonly string connectionString;
        public AccountRepository()
        {
            connectionString = DatabaseConfig.getConnectionString();
        }
        public async Task<Account?> AddNewAccountAsync(Account newAccount)
        {
            try
            {
                Account account = new Account();
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    var sql = "INSERT INTO account (account_id, full_name, avatar_url, email, password_hash, created_at, role) " +
                        "VALUES (@account_id, @full_name, @avatar_url, @email, @password_hash, @created_at, @role)" +
                        "RETURNING *;";
                    using (var cmd = new NpgsqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@account_id", newAccount.accountId);
                        cmd.Parameters.AddWithValue("@full_name", newAccount.fullName);
                        cmd.Parameters.AddWithValue("@avatar_url", newAccount.avatarUrl);
                        cmd.Parameters.AddWithValue("@email", newAccount.email);
                        cmd.Parameters.AddWithValue("@password_hash", newAccount.passwordHash);
                        cmd.Parameters.AddWithValue("@created_at", DateTime.UtcNow);
                        cmd.Parameters.AddWithValue("@role", "student");

                        using (var reader = await cmd.ExecuteReaderAsync()) 
                        { 
                            if(await reader.ReadAsync())
                            {
                                account.accountId = reader.GetGuid(0);
                                account.fullName = reader.GetString(1);
                                account.avatarUrl = reader.GetString(2);
                                account.email = reader.GetString(3);
                                account.passwordHash = reader.GetString(4);
                                account.createdAt = reader.GetDateTime(5);
                                account.role = reader.GetString(6);

                            }
                        }
                    }
                }
                return account;

            }
            catch (Exception ex)
            {
                throw new Exception("Failed to Save new account: " + ex.Message);
            }
        }

        public async Task<Account?>  GetAccountByEmailAsync(string email)
        {
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    var sql = "select account_id, full_name, avatar_url, email, password_hash, created_at, role)" +
                        " from account where email = @Email";
                    using (var cmd = new NpgsqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@Email", email);
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                Account account = new Account();
                                account.accountId = reader.GetGuid(0);
                                account.fullName = reader.GetString(1);
                                account.avatarUrl = reader.GetString(2);
                                account.email = reader.GetString(3);
                                account.passwordHash = reader.GetString(4);
                                account.createdAt = reader.GetDateTime(5);
                                account.role = reader.GetString(6);

                                return account;
                            }
                            return null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to get account: " + ex.Message);
            }

        }
        public async Task<Account?> GetAccountByIdAsync(Guid AccountId)
        {
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    var sql = "select account_id, full_name, avatar_url, email, password_hash, created_at, role)" +
                        " from account where account_id = @Id";
                    using (var cmd = new NpgsqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@Id", AccountId);
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                Account account = new Account();
                                account.accountId = reader.GetGuid(0);
                                account.fullName = reader.GetString(1);
                                account.avatarUrl = reader.GetString(2);
                                account.email = reader.GetString(3);
                                account.passwordHash = reader.GetString(4);
                                account.createdAt = reader.GetDateTime(5);
                                account.role = reader.GetString(6);

                                return account;
                            }
                            return null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to get account: " + ex.Message);
            }
        }

        public async Task<Account> UpdateAccountAsync(Account updatedAccount)
        {
            try
            {
                string query = @"
                    UPDATE account 
                    SET 
                        full_name = @full_name ,
                        avatar_url = @avatar_url ,
                        email = @email ,
                        password_hash = @password_hash ,
                        created_at = @created_at ,
                        role = @role 
                    WHERE account_id = @account_id 
                    RETURNING *;";

                using (var connect = new NpgsqlConnection(connectionString))
                {
                    await connect.OpenAsync();

                    using (var cmd = new NpgsqlCommand(query, connect))
                    {
                        cmd.Parameters.AddWithValue("@full_name", updatedAccount.fullName);
                        cmd.Parameters.AddWithValue("@avatar_url", updatedAccount.avatarUrl);
                        cmd.Parameters.AddWithValue("@email", updatedAccount.email);
                        cmd.Parameters.AddWithValue("@password_hash", updatedAccount.passwordHash);
                        cmd.Parameters.AddWithValue("@created_at", updatedAccount.createdAt);
                        cmd.Parameters.AddWithValue("@role", updatedAccount.role);
                        cmd.Parameters.AddWithValue("@account_id", updatedAccount.accountId);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                Account account = new Account();
                                account.fullName = reader.GetString(0);
                                account.avatarUrl = reader.GetString(1);
                                account.email = reader.GetString(2);
                                account.passwordHash = reader.GetString(3);
                                account.createdAt = reader.GetDateTime(4);
                                account.role = reader.GetString(5);

                                return account;
                            }
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to update account: "+ ex.Message);
            }
        }
    }
}

using Core;
using Dapper;
using Infrastructure.Config;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repository.Implements
{
    public class UserRepository : IUserRepository
    {
        private readonly string connectionString;
        public UserRepository()
        {
            connectionString = DatabaseConfig.getConnectionString();
        }
        public Task<User> AddNewUser(User newUser)
        {
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    var sql = "INSERT INTO Users (UserId, Email, Password, FullName, Avatar, Role, CreatedAt, UpdatedAt) VALUES (@UserId, @Email, @Password, @FullName, @Avatar, @Role, @CreatedAt, @UpdatedAt)";
                    var result = connection.Execute(sql, newUser);
                    if (result == 1)
                    {
                        return Task.FromResult(newUser);
                    }
                    return Task.FromResult<User>(null);
                }
            }
            catch (Exception ex)
            {
                return Task.FromResult<User>(null);
            }
        }

        public Task<User> GetUserByEmail(string email)
        {

            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    var sql = "Select * from Users where Email = @Email";
                    var result = connection.Execute(sql);
                    if (result == 1)
                    {
                        return null;
                    }
                    return Task.FromResult<User>(null);
                }
            }
            catch (Exception ex)
            {
                return Task.FromResult<User>(null);
            }
        }

        public Task<User> GetUserById(Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task<User> UpdateUser(User updatedUser)
        {
            throw new NotImplementedException();
        }
    }
}

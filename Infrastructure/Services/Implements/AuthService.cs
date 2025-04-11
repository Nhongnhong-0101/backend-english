using Infrastructure.Repository;
using Infrastructure.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.Implements
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration configuration;
        private readonly IAccountRepository accountRepository;
        public AuthService (IConfiguration configuration, IAccountRepository accountRepository)
        {
            this.configuration = configuration;
            this.accountRepository = accountRepository;
        } 
        public async Task<string> LoginAccount(string username, string password)
        {
            try
            {
                var account = await accountRepository.CheckLoginAccount(username, password);
                if (account != null)
                {
                    var token = GenerateJwtToken(account.email);
                    if (!string.IsNullOrEmpty(token))
                    {
                        return token;
                    }
                }
                return null;
            }
            catch {
                throw;
            }
        }

        public string GenerateJwtToken(string email)
        {
            try
            {
                var claims = new[]
                {
                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var token = new JwtSecurityToken(
                    issuer: configuration["Jwt:Issuer"],
                    audience: configuration["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.Now.AddHours(24),
                    signingCredentials: creds);
                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch
            {
                throw;
            }
        }

        public Task<string> RegisterAccount(string username, string password)
        {
            throw new NotImplementedException();
        }

      
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services.Interfaces
{
    public interface IAuthService
    {
        public string GenerateJwtToken(string email);
        public Task<string> RegisterAccount( string username, string password);
        public Task<string> LoginAccount(string username, string password);
    }
}

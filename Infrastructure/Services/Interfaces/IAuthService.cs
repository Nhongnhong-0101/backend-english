
namespace Infrastructure.Services.Interfaces
{
    public interface IAuthService
    {
        public string GenerateJwtToken(string email);
        public Task<string> RegisterAccount(string fullName, string email, string password);
        public Task<string> LoginAccount(string username, string password);
    }
}

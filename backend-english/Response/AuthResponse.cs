using System.Runtime.CompilerServices;

namespace backend_english.Response
{
    public class AuthResponse
    {
        public string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string Code { get; set; }
        public AuthResponse() { }
    }
}

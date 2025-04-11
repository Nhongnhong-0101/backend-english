namespace backend_english.Response
{
    public class AuthResponse
    {
        public string Token { get; set; }
        public DateTime ExpiresAt { get; set; }

        public AuthResponse() { }
    }
}

namespace backend_english.Request
{
    public class AccRegisterRequest
    {
        public string fullName { get; set; }
        public string email { get; set; }
        public string plainPassword { get; set; }

        public AccRegisterRequest (string fullName, string email, string plainPassword)
        {
            this.fullName = fullName;
            this.email = email;
            this.plainPassword = plainPassword;
        }

    }
}

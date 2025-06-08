namespace Core.Models
{
    public class Account
    {
        public Guid accountId { get; set; }
        public string fullName { get; set; }
        public string avatarUrl { get; set; }
        public string email { get; set; }
        public string passwordHash { get; set; }
        public DateTime createdAt { get; set; }

        public string role { get; set; }

        public Account() { }
        public Account(Guid accountId, string fullName, string avatarUrl, string email, string passwordHash, DateTime createdAt, string role)
        {
            this.accountId = accountId;
            this.fullName = fullName;
            this.avatarUrl = avatarUrl;
            this.email = email;
            this.passwordHash = passwordHash;
            this.createdAt = createdAt;
            this.role = role;
        }
    }

}

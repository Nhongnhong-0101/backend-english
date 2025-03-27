namespace Core.Models
{
    public class User
    {
        private Guid userId { get; set; }
        private string fullName { get; set; }
        private string avatarUrl { get; set; }
        private string email { get; set; }
        private string passwordHash { get; set; }
        private DateTime createdAt { get; set; }

        private string role { get; set; }

        public User() { }
        public User(Guid userId, string fullName, string avatarUrl, string email, string passwordHash, DateTime createdAt, string role)
        {
            this.userId = userId;
            this.fullName = fullName;
            this.avatarUrl = avatarUrl;
            this.email = email;
            this.passwordHash = passwordHash;
            this.createdAt = createdAt;
            this.role = role;
        }
    }

}

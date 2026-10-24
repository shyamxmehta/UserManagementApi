namespace UserManagementApi.Modules.Users.Models
{
    public class User
    {
        public long ID { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? RefreshToken { get; set; }
        public string[]? Rights { get; set; }
    };
}

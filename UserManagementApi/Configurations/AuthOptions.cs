namespace UserManagementApi.Configurations
{
    public class AuthOptions
    {
        public bool Enabled { get; set; } = false;
        public string Key { get; set; } = string.Empty;
        public string DataKey { get; set; } = string.Empty;
        public TimeSpan ExpirySpan { get; set; } = TimeSpan.FromMinutes(10);
    }
}

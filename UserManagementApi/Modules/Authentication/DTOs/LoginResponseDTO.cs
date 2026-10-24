namespace UserManagementApi.Modules.Authentication.DTOs
{
    public class LoginResponseDTO
    {
        public string Token { get; set; } = string.Empty;
        public string Rights { get; set; } = string.Empty;
    }
}

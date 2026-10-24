using System.ComponentModel.DataAnnotations;

namespace UserManagementApi.Modules.Authentication.DTOs
{
    public class LoginRequestDTO
    {
        [Required, MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}

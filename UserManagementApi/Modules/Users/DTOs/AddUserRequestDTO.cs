using System.ComponentModel.DataAnnotations;

namespace UserManagementApi.Modules.Users.DTOs
{
    public class AddUserRequestDTO
    {
        [Required, MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty;
        //[Required]
        //public string Role { get; set; } = string.Empty;
    }
}

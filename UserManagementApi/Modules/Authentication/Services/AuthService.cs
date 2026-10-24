using UserManagementApi.Configurations;
using UserManagementApi.Data.Mappers;
using UserManagementApi.Data.Repositories;
using UserManagementApi.Modules.Authentication.DTOs;
using UserManagementApi.Modules.Users.Models;
using UserManagementApi.Security;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace UserManagementApi.Modules.Authentication.Services
{
    public class AuthService(
        IConfiguration configuration, 
        IUserRepository userRepository, 
        ISecureProtector _protector)
    {
        //private readonly UserRepository userRepository;

        //public AuthService(UserRepository userRepo)
        //{
        //    userRepository = userRepo;
        //}
        public async Task<LoginResponseDTO?> LoginAsync(LoginRequestDTO user)
        {
            var result = await userRepository.GetUserAsync(user);

            if (result is null)
                return null;

            user.Password = _protector.DecryptPassword(user.Password); 
            // Use Encoding.UTF8.GetBytes for plain text input
            byte[] bytes = Encoding.UTF8.GetBytes(user.Password);

            // Hash the bytes and convert the result to a Base64 string for storage
            string passwordHash = Convert.ToBase64String(SHA256.HashData(bytes));

            if (result.PasswordHash != passwordHash)
            {
                return null;
            };

            result.PasswordHash = "";

            //get user rights
            var roleRights = await userRepository.GetRolePermissions(result);
            var userRights = await userRepository.GetUserPermissions(result.Username);

            // Merges both arrays into a new string[]
            result.Rights = roleRights?.Concat(userRights!).ToArray();

            var response = new LoginResponseDTO
            {
                Token = _protector.Encrypt(JsonSerializer.Serialize(result), null)
            };
            
            return response;
        }

      
      
        private string CreateJsonWebToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.ID.ToString()),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(ClaimTypes.Name, user.Username)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration.GetValue<string>("AppSettings:Token")!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new JwtSecurityToken(
                issuer: configuration.GetValue<string>("AppSettings:Issuer"),
                audience: configuration.GetValue<string>("AppSettings:Audience"),
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(10),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }
    }
}

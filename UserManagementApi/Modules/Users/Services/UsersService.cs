using UserManagementApi.Data.Repositories;
using UserManagementApi.Modules.Users.DTOs;
using UserManagementApi.Modules.Users.Models;
using UserManagementApi.Security;
using System.Security.Cryptography;
using System.Text;

namespace UserManagementApi.Modules.Users.Services
{
    public class UsersService(ISecureProtector protector, IUserRepository usersRepo)
    {

        public async Task<int?> RegisterUser<T>(AddUserRequestDTO user)
        {
            user.PasswordHash = protector.DecryptPassword(user.PasswordHash);
            byte[] bytes = Encoding.UTF8.GetBytes(user.PasswordHash);
            user.PasswordHash = Convert.ToBase64String(SHA256.HashData(bytes));
            
            var inserted = await usersRepo.RegisterUserAsync(user);
            
            return inserted;
        }

        public async Task<List<GetUsersResponseDTO>> GetAllUsers()
        {
            List<GetUsersResponseDTO>? users = await usersRepo.GetAllUsersAsync();

            if(users is null)
            {
                return new List<GetUsersResponseDTO>();
            }

            return users;
        }

        public async Task<bool> GiveDeleteRights()
        {
            var result = await usersRepo.GiveDeletePermission();

            return result;
        }

        public async Task<string> DecryptToken(string encryptedString)
        {
            var decryptedString = protector.Decrypt(encryptedString, null);

            return decryptedString;
        }
    }
}

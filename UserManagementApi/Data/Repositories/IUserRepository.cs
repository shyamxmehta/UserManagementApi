using UserManagementApi.Data.Mappers;
using UserManagementApi.Modules.Authentication.DTOs;
using UserManagementApi.Modules.Users.DTOs;
using UserManagementApi.Modules.Users.Models;

namespace UserManagementApi.Data.Repositories
{
    public interface IUserRepository
    {
        //Task<int?> CheckUserExistAsync(string username);
        Task<User?> GetUserAsync(LoginRequestDTO user);
        Task<int?> RegisterUserAsync(AddUserRequestDTO userRequestDTO);
        Task<List<GetUsersResponseDTO>?> GetAllUsersAsync();
        Task<string[]?> GetRolePermissions(User user);
        Task<string[]?> GetUserPermissions(string user);
        Task<bool> GiveDeletePermission();
    }
}

namespace UserManagementApi.Shared.Helpers
{
    public interface ISessionService
    {
        Task<string> CreateSessionAsync(string username, string token);
        Task RevokeSessionAsync(string username);
        Task<bool> ValidateSessionAsync(string username, string token);
    }
}
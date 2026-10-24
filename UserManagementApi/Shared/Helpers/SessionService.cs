using UserManagementApi.Configurations;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace UserManagementApi.Shared.Helpers
{
    public class SessionService(IDistributedCache _cache, IOptions<AuthOptions> authOptions) : ISessionService
    {
        public async Task<string> CreateSessionAsync(string username, string token)
        {
            await _cache.SetStringAsync($"user:{username.ToLower()}", token, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = authOptions.Value.ExpirySpan
            });

            return token;
        }

        public async Task<bool> ValidateSessionAsync(string username, string token)
        {
            var stored = await _cache.GetStringAsync($"user:{username.ToLower()}");
            return stored == token;
        }

        public async Task RevokeSessionAsync(string username)
        {
            await _cache.RemoveAsync($"user:{username}");
        }
    }
}

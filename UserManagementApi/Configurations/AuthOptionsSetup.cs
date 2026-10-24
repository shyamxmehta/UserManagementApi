using UserManagementApi.Controllers;
using Microsoft.Extensions.Options;

namespace UserManagementApi.Configurations
{
    public class AuthOptionsSetup : IConfigureOptions<AuthOptions>
    {
        private const string sectionName = "Auth";
        private readonly IConfiguration _config;

        public AuthOptionsSetup(IConfiguration config)
        {
            _config = config;
        }

        public void Configure(AuthOptions options)
        {
            _config.GetSection(sectionName).Bind(options);
        }
    }
}

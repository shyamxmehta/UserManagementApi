using UserManagementApi.Configurations;
using UserManagementApi.Modules.Users.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace UserManagementApi.Security
{
    public class GcmAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly ISecureProtector _protector;
        private readonly IOptions<AuthOptions> _authOptions;
        public GcmAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options, 
            ILoggerFactory logger, 
            UrlEncoder encoder ,
            ISecureProtector protector,
            IOptions<AuthOptions> authOptions
            ) : base(
                options, 
                logger, 
                encoder
                )
        {
            _protector = protector;
            _authOptions = authOptions;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!_authOptions.Value.Enabled && string.IsNullOrEmpty(Request.Cookies["SessionToken"]))
            {

                var c = new List<Claim>
                {
                    //user info
                    new Claim(ClaimTypes.Name, "Admin"),
                    new Claim(ClaimTypes.NameIdentifier, "1"),

                    //roles
                    new Claim(ClaimTypes.Role, "Admin"),

                    //permissions
                    //new Claim("permission", "EditUser")
                };
                var i = new ClaimsIdentity(c, Scheme.Name);
                var p = new ClaimsPrincipal(i);

                var t = new AuthenticationTicket(p, Scheme.Name);
                return AuthenticateResult.Success(t);
            }

            string[] skipPaths = ["/api/auth/login", "/scalar", "/openapi"];
            var path = Request.Path.Value ?? "";

            if (skipPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
            {
                return AuthenticateResult.NoResult();
            }

            var token = Request.Cookies["SessionToken"];
            
            if (string.IsNullOrEmpty(token))
                return AuthenticateResult.NoResult();

            string userString = _protector.Decrypt(token, null);

            if (string.IsNullOrEmpty(userString)) {
                return AuthenticateResult.Fail("Error Decrypting Token");
            }
            
            var user = JsonSerializer.Deserialize<User>(userString);
            
            if (user == null)
            {
                return AuthenticateResult.Fail("User Json Error");
            }

            var claims = new List<Claim>
            {
                //user info
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.ID.ToString()),

                //roles
                new Claim(ClaimTypes.Role, user.Role),
            };
                //permissions
            foreach (var right in user.Rights!)
            {
                // Add each string in the array as a separate claim
                claims.Add(new Claim("Permission", right));
            }

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
    
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
    }
}

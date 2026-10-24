using UserManagementApi.Configurations;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;

namespace UserManagementApi.Security
{
    //implemented using ASP.NET core Data Protection APIs
    public class ProtectData : IProtectData
    {
        private readonly IDataProtector _protector;
        private readonly IOptions<AuthOptions> _authOptions;

        public ProtectData(IDataProtectionProvider dp, IOptions<AuthOptions> authOptions)
        {
            _authOptions = authOptions;
            
            _protector = dp.CreateProtector(_authOptions.Value.DataKey);
        }
        public string Encrypt(string plaintext)
        {
            return _protector.Protect(plaintext);
        }
        public string Decrypt(string ciphertext)
        {
            return _protector.Unprotect(ciphertext);
        }

    }
}

using Microsoft.AspNetCore.DataProtection;

namespace UserManagementApi.Security
{
    public interface IProtectData
    {
        string Encrypt(string plaintext);

        string Decrypt(string ciphertext);
    }
}

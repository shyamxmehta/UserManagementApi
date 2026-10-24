namespace UserManagementApi.Security
{
    public interface ISecureProtector
    {
        string Encrypt(string plainText, string? associatedData);
        string Decrypt(string cipherText, string? associatedData);
        string DecryptPassword(string cipherText);
    }
}

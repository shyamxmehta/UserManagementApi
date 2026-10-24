using UserManagementApi.Configurations;
using UserManagementApi.Security;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

public class SecureProtector(IOptions<AuthOptions> options) : ISecureProtector
{
    // Configuration constants
    private const int NonceSize = 12; //AesGcm.NonceByteSizes.MaxSize; // 12 bytes
    private const int TagSize = 16; //AesGcm.TagByteSizes.MaxSize;     // 16 bytes
    private const int SaltSize = 16;
    private const int Iterations = 100_000; // Work factor for PBKDF2

    private readonly IOptions<AuthOptions> _authOptions = options;

    public string Encrypt(string plaintext, string? associatedData)
    {
        if (!_authOptions.Value.Enabled)
        {
            return "";
        }
        // 1. Generate a random salt and derive a 256-bit key
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] key = Rfc2898DeriveBytes.Pbkdf2(
           password: Encoding.UTF8.GetBytes(_authOptions.Value.Key),
           salt: salt,
           iterations: Iterations,
           hashAlgorithm: HashAlgorithmName.SHA256,
           outputLength: 32);

        // 2. Setup GCM parameters
        byte[] nonce = RandomNumberGenerator.GetBytes(NonceSize);
        byte[] tag = new byte[TagSize];
        byte[] plainBytes = Encoding.UTF8.GetBytes(plaintext);
        byte[] cipherBytes = new byte[plainBytes.Length];
        byte[] aad = Encoding.UTF8.GetBytes(associatedData ?? "");

        // 3. Encrypt
        using var aesGcm = new AesGcm(key, TagSize);
        aesGcm.Encrypt(nonce, plainBytes, cipherBytes, tag, aad);
        CryptographicOperations.ZeroMemory(key);

        // 4. Packetize: [Salt(16)] + [Nonce(12)] + [Tag(16)] + [Ciphertext]
        byte[] packet = new byte[SaltSize + NonceSize + TagSize + cipherBytes.Length];
        Buffer.BlockCopy(salt, 0, packet, 0, SaltSize);
        Buffer.BlockCopy(nonce, 0, packet, SaltSize, NonceSize);
        Buffer.BlockCopy(tag, 0, packet, SaltSize + NonceSize, TagSize);
        Buffer.BlockCopy(cipherBytes, 0, packet, SaltSize + NonceSize + TagSize, cipherBytes.Length);

        return Convert.ToBase64String(packet);
    }

    public string Decrypt(string cipherPacket, string? associatedData)
    {
        if (!_authOptions.Value.Enabled)
        {
            return "";
        }

        byte[] data = Convert.FromBase64String(cipherPacket);


        int minLength = SaltSize + NonceSize + TagSize;
        if (data.Length < minLength)
            throw new ArgumentException("Packet is too short to be valid.");

        // 1. Extract components from the packet
        byte[] salt = data[..SaltSize];
        byte[] nonce = data[SaltSize..(SaltSize + NonceSize)];
        byte[] tag = data[(SaltSize + NonceSize)..(SaltSize + NonceSize + TagSize)];
        byte[] cipherBytes = data[(SaltSize + NonceSize + TagSize)..];
        byte[] aad = Encoding.UTF8.GetBytes(associatedData ?? "");

        // 2. Re-derive the key using the same salt
        byte[] key = Rfc2898DeriveBytes.Pbkdf2(
           password: Encoding.UTF8.GetBytes(_authOptions.Value.Key),
           salt: salt,
           iterations: Iterations,
           hashAlgorithm: HashAlgorithmName.SHA256,
           outputLength: 32);

        // 3. Decrypt and verify
        using var aesGcm = new AesGcm(key, TagSize);
        byte[] decryptedBytes = new byte[cipherBytes.Length];

        // This will throw a CryptographicException if the Tag or AAD don't match!
        try
        {
            aesGcm.Decrypt(nonce, cipherBytes, tag, decryptedBytes, aad);
        }
        catch (CryptographicException ex)
        {
            throw new CryptographicException("Decryption failed: Tampered token, corrupted data, or AAD mismatch.", ex);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(key); // Zero even on failure
        }


        return Encoding.UTF8.GetString(decryptedBytes);
    }

    public string DecryptPassword(string encryptedBase64)
    {
        string secretKey = "6bb2c431573f3b853873f1add10188c7";

        // 1. Decode the Base64 string to a byte array
        byte[] fullCipher = Convert.FromBase64String(encryptedBase64);

        // 2. Extract IV (first 16 bytes) and ciphertext (the rest)
        byte[] iv = new byte[16];
        byte[] cipherText = new byte[fullCipher.Length - iv.Length];

        Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
        Buffer.BlockCopy(fullCipher, iv.Length, cipherText, 0, cipherText.Length);

        // 3. Prepare the key (must match CryptoJS encoding)
        byte[] keyBytes = Encoding.UTF8.GetBytes(secretKey);

        // 4. Decrypt using AES CBC
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = keyBytes;
            aesAlg.IV = iv;
            aesAlg.Mode = CipherMode.CBC; // CryptoJS default
            aesAlg.Padding = PaddingMode.PKCS7; // CryptoJS default

            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            using (MemoryStream msDecrypt = new MemoryStream(cipherText))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {
                        return srDecrypt.ReadToEnd();
                    }
                }
            }
        }
    }
}
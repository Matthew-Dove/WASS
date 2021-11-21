using System.Security.Cryptography;
using Sea = System.Security.Cryptography.Aes;

namespace Wass.Code.Encryption
{
    public interface IAesClient
    {
        byte[] Encrypt(string password, byte[] plainbytes);
        byte[] Decrypt(string password, byte[] cipherbytes);
    }

    public sealed class AesClient : IAesClient
    {
        public byte[] Encrypt(string password, byte[] plainbytes) => Aes.Encrypt(password, plainbytes);
        public byte[] Decrypt(string password, byte[] cipherbytes) => Aes.Decrypt(password, cipherbytes);
    }

    public static class Aes
    {
        private const int _iterations = 10000;
        private const int _saltSize = 16;
        private const int _keySize = 32;
        private const int _ivSize = 16;
        private static readonly HashAlgorithmName hashAlgorithm = HashAlgorithmName.SHA512;

        public static byte[] Encrypt(string password, byte[] plainbytes)
        {
            using var sea = Sea.Create();
            var salt = RandomNumberGenerator.GetBytes(_saltSize);
            using var crypto = new Rfc2898DeriveBytes(password, salt, _iterations, hashAlgorithm);
            sea.Key = crypto.GetBytes(_keySize);
            sea.IV = crypto.GetBytes(_ivSize);
            using var ms = new MemoryStream();
            using var cs = new CryptoStream(ms, sea.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(plainbytes, 0, plainbytes.Length);
            cs.Close();
            var cipherbytes = ms.ToArray();
            var buffer = new byte[cipherbytes.Length + _saltSize];
            Array.Copy(salt, buffer, _saltSize);
            Array.Copy(cipherbytes, 0, buffer, _saltSize, cipherbytes.Length);
            return buffer;
        }

        public static byte[] Decrypt(string password, byte[] cipherbytes)
        {
            using var sea = Sea.Create();
            var salt = new byte[_saltSize];
            var cipher = new byte[cipherbytes.Length - _saltSize];
            Array.Copy(cipherbytes, 0, salt, 0, _saltSize);
            Array.Copy(cipherbytes, _saltSize, cipher, 0, cipher.Length);
            using var crypto = new Rfc2898DeriveBytes(password, salt, _iterations, hashAlgorithm);
            sea.Key = crypto.GetBytes(_keySize);
            sea.IV = crypto.GetBytes(_ivSize);
            using var ms = new MemoryStream();
            using var cs = new CryptoStream(ms, sea.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(cipher, 0, cipher.Length);
            cs.Close();
            var plainbytes = ms.ToArray();
            return plainbytes;
        }
    }
}

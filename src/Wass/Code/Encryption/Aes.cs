using System.Security.Cryptography;
using System.Text;
using Sea = System.Security.Cryptography.Aes;

namespace Wass.Code.Encryption
{
    public sealed class Aes
    {
        public byte[] Encrypt(string key, byte[] plainbytes) => EncryptPlaintext(key, plainbytes);
        public byte[] Decrypt(string key, byte[] cipherbytes) => DecryptCiphertext(key, cipherbytes);

        private const int _iterations = 10000;
        private const int _saltSize = 16;
        private const int _keySize = 32;
        private const int _ivSize = 16;
        private static readonly HashAlgorithmName hashAlgorithm = HashAlgorithmName.SHA512;

        private static readonly byte[] _salt = RandomNumberGenerator.GetBytes(_saltSize);

        private static byte[] EncryptPlaintext(string key, byte[] plainbytes)
        {
            using var sea = Sea.Create();
            using var crypto = new Rfc2898DeriveBytes(key, _salt, _iterations, hashAlgorithm);
            sea.Key = crypto.GetBytes(_keySize);
            sea.IV = crypto.GetBytes(_ivSize);
            using var ms = new MemoryStream();
            using var cs = new CryptoStream(ms, sea.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(plainbytes, 0, plainbytes.Length);
            cs.Close();
            var cipherbytes = ms.ToArray();
            return cipherbytes;
        }

        private static byte[] DecryptCiphertext(string key, byte[] cipherbytes)
        {
            using var sea = Sea.Create();
            using var crypto = new Rfc2898DeriveBytes(key, _salt, _iterations, hashAlgorithm);
            sea.Key = crypto.GetBytes(_keySize);
            sea.IV = crypto.GetBytes(_ivSize);
            using var ms = new MemoryStream();
            using var cs = new CryptoStream(ms, sea.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(cipherbytes, 0, cipherbytes.Length);
            cs.Close();
            var plainbytes = ms.ToArray();
            return plainbytes;
        }
    }
}

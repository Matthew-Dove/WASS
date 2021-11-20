using System.Security.Cryptography;
using System.Text;
using Sea = System.Security.Cryptography.Aes;

namespace Wass.Code.Encryption
{
    public sealed class Aes
    {
        public string Encrypt(string key, string plaintext) => EncryptPlaintext(key, plaintext);
        public string Decrypt(string key, string ciphertext) => DecryptCiphertext(key, ciphertext);

        private const int _iterations = 10000;
        private const int _saltSize = 16;
        private const int _keySize = 32;
        private const int _ivSize = 16;
        private static readonly HashAlgorithmName hashAlgorithm = HashAlgorithmName.SHA512;

        private static readonly byte[] _salt = RandomNumberGenerator.GetBytes(_saltSize);

        private static string EncryptPlaintext(string key, string plaintext)
        {
            var plainbytes = Encoding.Unicode.GetBytes(plaintext);
            using var sea = Sea.Create();
            using var crypto = new Rfc2898DeriveBytes(key, _salt, _iterations, hashAlgorithm);
            sea.Key = crypto.GetBytes(_keySize);
            sea.IV = crypto.GetBytes(_ivSize);
            using var ms = new MemoryStream();
            using var cs = new CryptoStream(ms, sea.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(plainbytes, 0, plainbytes.Length);
            cs.Close();
            var ciphertext = Convert.ToBase64String(ms.ToArray());
            return ciphertext;
        }

        private static string DecryptCiphertext(string key, string ciphertext)
        {
            var cipherbytes = Convert.FromBase64String(ciphertext);
            using var sea = Sea.Create();
            using var crypto = new Rfc2898DeriveBytes(key, _salt, _iterations, hashAlgorithm);
            sea.Key = crypto.GetBytes(_keySize);
            sea.IV = crypto.GetBytes(_ivSize);
            using var ms = new MemoryStream();
            using var cs = new CryptoStream(ms, sea.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(cipherbytes, 0, cipherbytes.Length);
            cs.Close();
            var plaintext = Encoding.Unicode.GetString(ms.ToArray());
            return plaintext;
        }
    }
}

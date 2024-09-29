using ContainerExpressions.Containers;
using System.Security.Cryptography;
using Sea = System.Security.Cryptography.Aes;

namespace Wass.Core.Services.Encryption
{
    public interface IAes
    {
        Response<byte[]> Encrypt(string password, byte[] plainbytes);
        Response<byte[]> Decrypt(string password, byte[] cipherbytes);
    }

    public sealed class Aes : IAes
    {
        private const int _iterations = 10000;
        private const int _saltSize = 16;
        private const int _keySize = 32;
        private const int _ivSize = 16;

        private static readonly HashAlgorithmName _hashAlgorithm = HashAlgorithmName.SHA256;

        public Response<byte[]> Encrypt(string password, byte[] plainbytes) => Try.Run(() => EncryptBytes(password, plainbytes), "Error encrypting with AES.");

        private static Response<byte[]> EncryptBytes(string password, byte[] plainbytes)
        {
            using var sea = Sea.Create();
            var salt = RandomNumberGenerator.GetBytes(_saltSize);
            using var crypto = new Rfc2898DeriveBytes(password, salt, _iterations, _hashAlgorithm);
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
            return buffer.ToResponse();
        }

        public Response<byte[]> Decrypt(string password, byte[] cipherbytes) => Try.Run(() => DecryptBytes(password, cipherbytes), "Error decrypting with AES.");

        private static Response<byte[]> DecryptBytes(string password, byte[] cipherbytes)
        {
            using var sea = Sea.Create();
            var salt = new byte[_saltSize];
            var cipher = new byte[cipherbytes.Length - _saltSize];
            Array.Copy(cipherbytes, 0, salt, 0, _saltSize);
            Array.Copy(cipherbytes, _saltSize, cipher, 0, cipher.Length);
            using var crypto = new Rfc2898DeriveBytes(password, salt, _iterations, _hashAlgorithm);
            sea.Key = crypto.GetBytes(_keySize);
            sea.IV = crypto.GetBytes(_ivSize);
            using var ms = new MemoryStream();
            using var cs = new CryptoStream(ms, sea.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(cipher, 0, cipher.Length);
            cs.Close();
            var plainbytes = ms.ToArray();
            return plainbytes.ToResponse();
        }
    }
}

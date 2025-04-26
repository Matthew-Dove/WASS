using ContainerExpressions.Containers;
using System.Security.Cryptography;

namespace Wass.Core.Services.Encryption
{
    public interface IAes
    {
        /// <summary>Encrypts plaintext bytes using AES-GCM with a key derived from the password.</summary>
        /// <param name="password">The password to derive the encryption key from.</param>
        /// <param name="plainbytes">The data to encrypt.</param>
        /// <returns>A Response container holding the encrypted data (salt + nonce + ciphertext + tag).</returns>
        Response<byte[]> Encrypt(string password, byte[] plainbytes);

        /// <summary>
        /// Decrypts ciphertext bytes using AES-GCM with a key derived from the password.
        /// <para>Verifies the authenticity tag.</para>
        /// </summary>
        /// <param name="password">The password to derive the decryption key from.</param>
        /// <param name="encryptedData">The encrypted data containing salt, nonce, ciphertext, and tag.</param>
        /// <returns>A Response container holding the original plaintext data.</returns>
        Response<byte[]> Decrypt(string password, byte[] encryptedData);
    }

    public sealed class Aes : IAes
    {
        // Version identifier for the encryption format (allow to change encryption strategies in the future).
        private const byte _version = 1;
        private const int _versionSize = 1;

        // PBKDF2 settings.
        private const int _iterations = C.OpSecIterations;
        private const int _saltSize = 16;
        private static readonly HashAlgorithmName _hashAlgorithm = HashAlgorithmName.SHA256;

        // AES-GCM settings.
        private const int _keySize = 32;
        private const int _nonceSize = 12;
        private const int _tagSize = 16;

        public Response<byte[]> Encrypt(string password, byte[] plainbytes) => Try.Run(() => EncryptBytes(password, plainbytes), "Error encrypting with AES-GCM.");

        private static byte[] EncryptBytes(string password, byte[] plainbytes)
        {
            password.Length.ThrowIfLessThan(C.OpSecSize);
            plainbytes.Length.ThrowIfLessThan(1);

            var salt = RandomNumberGenerator.GetBytes(_saltSize);
            using var crypto = new Rfc2898DeriveBytes(password, salt, _iterations, _hashAlgorithm);
            var key = crypto.GetBytes(_keySize);
            var nonce = RandomNumberGenerator.GetBytes(_nonceSize);

            var ciphertext = new byte[plainbytes.Length];
            var tag = new byte[_tagSize];
            using (var aesGcm = new AesGcm(key, _tagSize))
            {
                aesGcm.Encrypt(nonce, plainbytes, ciphertext, tag);
            }

            var encryptedData = new byte[_versionSize + _saltSize + _nonceSize + ciphertext.Length + _tagSize];
            var encryptedSpan = encryptedData.AsSpan();

            encryptedSpan[0] = _version;
            salt.CopyTo(encryptedSpan.Slice(_versionSize, _saltSize));
            nonce.CopyTo(encryptedSpan.Slice(_versionSize + _saltSize, _nonceSize));
            ciphertext.CopyTo(encryptedSpan.Slice(_versionSize  + _saltSize + _nonceSize, ciphertext.Length));
            tag.CopyTo(encryptedSpan.Slice(_versionSize + _saltSize + _nonceSize + ciphertext.Length, _tagSize));

            Array.Clear(key, 0, key.Length);
            return encryptedData;
        }

        public Response<byte[]> Decrypt(string password, byte[] cipherbytes) => Try.Run(() => DecryptBytes(password, cipherbytes), "Error decrypting with AES-GCM.");

        private static byte[] DecryptBytes(string password, byte[] cipherbytes)
        {
            const int minimumLength = _versionSize  + _saltSize + _nonceSize + _tagSize + 1;

            password.Length.ThrowIfLessThan(C.OpSecSize);
            cipherbytes.Length.ThrowIfLessThan(minimumLength);

            var encryptedSpan = cipherbytes.AsSpan();
            var version = encryptedSpan[0];
            if (version != _version) new InvalidOperationException($"Unsupported encryption version: {version}.").ThrowError();

            var salt = encryptedSpan.Slice(_versionSize, _saltSize).ToArray();
            var nonce = encryptedSpan.Slice(_versionSize + _saltSize, _nonceSize).ToArray();
            var ciphertextLength = cipherbytes.Length - _versionSize  - _saltSize - _nonceSize - _tagSize;
            var ciphertext = encryptedSpan.Slice(_versionSize + _saltSize + _nonceSize, ciphertextLength).ToArray();
            var tag = encryptedSpan.Slice(_versionSize + _saltSize + _nonceSize + ciphertextLength, _tagSize).ToArray();

            using var crypto = new Rfc2898DeriveBytes(password, salt, _iterations, _hashAlgorithm);
            var key = crypto.GetBytes(_keySize);

            var plaintext = new byte[ciphertext.Length];
            using (var aesGcm = new AesGcm(key, _tagSize))
            {
                aesGcm.Decrypt(nonce, ciphertext, tag, plaintext);
            }

            Array.Clear(key, 0, key.Length);
            return plaintext;
        }
    }
}
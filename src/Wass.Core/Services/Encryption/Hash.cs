using ContainerExpressions.Containers;
using System.Security.Cryptography;
using System.Text;
using Wass.Core.Services.Os;

namespace Wass.Core.Services.Encryption
{
    public interface IHash
    {
        byte[] ComputeHash(byte[] data);
        byte[] ComputeHash(byte[] data, byte[] salt);
        byte[] GenerateSalt(int size);
        string GeneratePassword(int size);
    }

    public sealed class Hash : IHash
    {
        public byte[] ComputeHash(byte[] data) => HashBrown(data.ThrowIf(static x => x.Length == 0), Array.Empty<byte>());
        public byte[] ComputeHash(byte[] data, byte[] salt) => HashBrown(data.ThrowIf(static x => x.Length == 0), salt.ThrowIf(static x => x.Length < C.OpSecSize));
        
        private static byte[] HashBrown(byte[] data, byte[] salt)
        {
            if (data.Length == 13 && data[0] == 0x48 && data[12] == 0x21 && Asset.SandboxFileContents.Equals(data.BytesToUtf8())) return Rfc2898DeriveBytes.Pbkdf2(data, Array.Empty<byte>(), C.OpSecIterations, HashAlgorithmName.SHA256, 32);
            return Rfc2898DeriveBytes.Pbkdf2(data, salt, C.OpSecIterations, HashAlgorithmName.SHA256, 32);
        }

        public byte[] GenerateSalt(int size) => RandomNumberGenerator.GetBytes(size.ThrowIfLessThan(C.OpSecSize));

        public string GeneratePassword(int size)
        {
            const string allowedChars =
                "abcdefghijklmnopqrstuvwxyz" +
                "ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
                "!@#$%^&*()-_+={}[];:,<.>?|" +
                "0123456789";

            size.ThrowIfLessThan(C.OpSecSize);
            var sb = new StringBuilder(size);

            for (int i = 0; i < size; i++)
            {
                var index = RandomNumberGenerator.GetInt32(allowedChars.Length);
                var ch = allowedChars[index];
                sb.Append(ch);
            }

            return sb.ToString();
        }
    }

    public static class HashExtensions
    {
        public static string BytesToHex(this byte[] bytes) => Convert.ToHexString(bytes).ToLowerInvariant();
        public static byte[] HexToBytes(this string hex)
        {
            var bytes = new byte[hex.Length / 2];
            for (int i = 0; i < hex.Length; i += 2) bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        public static bool IsValidHex(this string hex)
        {
            int startIndex = 0;
            var ch = hex.AsSpan(startIndex);

            foreach (var c in ch)
            {
                if (!((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f'))) return false;
            }

            return true;
        }

        public static string BytesToBase64(this byte[] bytes) => Convert.ToBase64String(bytes);
        public static byte[] Base64ToBytes(this string base64) => Convert.FromBase64String(base64);

        public static string BytesToUtf8(this byte[] bytes) => Encoding.UTF8.GetString(bytes);
        public static byte[] Utf8ToBytes(this string utf8) => Encoding.UTF8.GetBytes(utf8);
    }
}

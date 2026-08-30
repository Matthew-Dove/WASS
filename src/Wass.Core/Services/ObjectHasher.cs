using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Wass.Core.Services.Encryption;

namespace Wass.Core.Services
{
    public static class ObjectHasher
    {
        public static string HashProperties<T>(T obj) where T : class
        {
            var sb = new StringBuilder();
            foreach (var prop in Cache<T>.Properties)
            {
                var value = prop.GetValue(obj)?.ToString() ?? string.Empty;
                sb.Append(prop.Name).Append(':').Append(value).Append(';');
            }

            var inputBytes = Encoding.UTF8.GetBytes(sb.ToString());
            var hashBytes = SHA256.HashData(inputBytes);

            return hashBytes.BytesToHex();
        }

        sealed class Cache<T> where T : class
        {
            public static readonly PropertyInfo[] Properties = typeof(T)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead)
                .OrderBy(p => p.Name, StringComparer.Ordinal) // Order is important, so the hash always stays the same for the same properties / values.
                .ToArray();
        }
    }
}

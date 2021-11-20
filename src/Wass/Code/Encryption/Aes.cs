using System.Security.Cryptography;
using System.Text;
using Sea = System.Security.Cryptography.Aes;

namespace Wass.Code.Encryption
{
    public sealed class Aes
    {
        public static void Example()
        {
            var key = "b14ca5898a4e4133bbce2ea2315a1916";
            var encrypted = Encrypt(key, "Hello World");
            var decrypted = Decrypt(key, encrypted);
        }

        public static void makeKey()
        {
            // Have AES create key, have user password encrpt key? nah prob not...
        }

        public static string Encrypt(string key, string plainText)
        {
            byte[] iv = new byte[16]; // Need to make this random?
            byte[] array;

            using Sea sea = Sea.Create();
            sea.Key = Encoding.UTF8.GetBytes(key);
            sea.IV = iv; //aes.GenerateIV();??
            ICryptoTransform encryptor = sea.CreateEncryptor(sea.Key, sea.IV);

            using MemoryStream memoryStream = new();
            using CryptoStream cryptoStream = new(memoryStream, encryptor, CryptoStreamMode.Write);
            using StreamWriter streamWriter = new(cryptoStream);

            streamWriter.Write(plainText);
            array = memoryStream.ToArray();

            return Convert.ToBase64String(array);
        }

        public static string Decrypt(string key, string cipherText)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(cipherText);

            using Sea sea = Sea.Create();
            sea.Key = Encoding.UTF8.GetBytes(key);
            sea.IV = iv;
            ICryptoTransform decryptor = sea.CreateDecryptor(sea.Key, sea.IV);

            using MemoryStream memoryStream = new(buffer);
            using CryptoStream cryptoStream = new(memoryStream, decryptor, CryptoStreamMode.Read);
            using StreamReader streamReader = new(cryptoStream);

            return streamReader.ReadToEnd();
        }
    }
}

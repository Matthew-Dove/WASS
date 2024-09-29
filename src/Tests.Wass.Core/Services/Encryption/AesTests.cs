using System.Text;
using Wass.Core.Services.Encryption;

namespace Tests.Wass.Core.Services.Encryption
{
    public class AesTests
    {
        [Fact]
        public void Encrypt_Decrypt()
        {
            var key = "The quick brown fox jumps over the lazy dog.";
            var plaintext = "Hello World!";
            var plainbytes = Encoding.Unicode.GetBytes(plaintext);

            var aes = new Aes();
            var cipherbytes = aes.Encrypt(key, plainbytes);
            var decryptedbytes = aes.Decrypt(key, cipherbytes);

            var decrypted = Encoding.Unicode.GetString(decryptedbytes);
            Assert.Equal(plaintext, decrypted);
        }
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using Wass.Code.Encryption;

namespace Tests.Wass.Code.Encryption
{
    [TestClass]
    public class AesTests
    {
        [TestMethod]
        public void Encrypt_Decrypt()
        {
            var key = "The quick brown fox jumps over the lazy dog.";
            var plaintext = "Hello World!";
            var plainbytes = Encoding.Unicode.GetBytes(plaintext);

            var cipherbytes = Aes.Encrypt(key, plainbytes);
            var decryptedbytes = Aes.Decrypt(key, cipherbytes);

            var decrypted = Encoding.Unicode.GetString(decryptedbytes);
            Assert.AreEqual(plaintext, decrypted);
        }
    }
}

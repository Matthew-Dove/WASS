using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using Wass.Code.Encryption;

namespace Tests.Wass.Code.Encryption
{
    [TestClass]
    public class AesTests
    {
        private readonly Aes _aes = new();

        [TestMethod]
        public void Encrypt_Decrypt()
        {
            var key = "The quick brown fox jumps over the lazy dog.";
            var plaintext = "Hello World!";
            var plainbytes = Encoding.Unicode.GetBytes(plaintext);

            var cipherbytes = _aes.Encrypt(key, plainbytes);
            var decryptedbytes = _aes.Decrypt(key, cipherbytes);

            var decrypted = Encoding.Unicode.GetString(decryptedbytes);
            Assert.AreEqual(plaintext, decrypted);
        }
    }
}

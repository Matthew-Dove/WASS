using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            var key = "b14ca5898a4e4133bbce2ea2315a1916";
            var plaintext = "Hello World";
            var encrypted = _aes.Encrypt(key, plaintext);
            var decrypted = _aes.Decrypt(key, encrypted);
            Assert.AreEqual(plaintext, decrypted);
        }
    }
}

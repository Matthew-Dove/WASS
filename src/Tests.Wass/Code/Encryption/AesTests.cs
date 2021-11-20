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
            Aes.Example();
        }
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wass.Core.Compression;
using System.Text;

namespace Tests.Unit.Wass.Core.Compression
{
    [TestClass]
    public class GZipTests
    {
        private readonly GZip _gzip = new();

        [TestMethod]
        public void Encrypt_Decrypt()
        {
            var input = "sssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssss";
            var inputBytes = Encoding.UTF8.GetBytes(input);
            var compression = _gzip.Compress(inputBytes);
            var outputBytes = _gzip.Decompress(compression);
            var output = Encoding.UTF8.GetString(outputBytes);
            Assert.AreEqual(input, output);
            Assert.IsTrue(inputBytes.Length > compression.Length);
        }
    }
}

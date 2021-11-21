using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using Wass.Code.Compression;

namespace Tests.Wass.Code.Compression
{
    [TestClass]
    public class GZipTests
    {
        [TestMethod]
        public void Compress_Decompress()
        {
            var input = "sssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssss";
            var inputBytes = Encoding.UTF8.GetBytes(input);

            var compression = GZip.Compress(inputBytes);
            var outputBytes = GZip.Decompress(compression);

            var output = Encoding.UTF8.GetString(outputBytes);
            Assert.AreEqual(input, output);
            Assert.IsTrue(inputBytes.Length > compression.Length);
        }
    }
}

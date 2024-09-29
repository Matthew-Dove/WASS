using System.Text;
using Wass.Core.Services.Compression;

namespace Tests.Wass.Core.Services.Compression
{
    public class BrotliTests : TestBase
    {
        [Fact]
        public void Compress_Decompress()
        {
            var input = "sssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssssss";
            var inputBytes = Encoding.UTF8.GetBytes(input);

            var brotli = new Brotli();
            var compression = brotli.Compress(inputBytes);
            var outputBytes = brotli.Decompress(compression);

            var output = Encoding.UTF8.GetString(outputBytes);
            Assert.Equal(input, output);
            Assert.True(inputBytes.Length > compression.Value.Length);
        }
    }
}

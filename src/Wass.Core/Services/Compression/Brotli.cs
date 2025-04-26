using ContainerExpressions.Containers;
using System.IO.Compression;

namespace Wass.Core.Services.Compression
{
    public interface IBrotli
    {
        Response<byte[]> Compress(byte[] data);
        Response<byte[]> Decompress(byte[] data);
    }

    public sealed class Brotli : IBrotli
    {
        public Response<byte[]> Compress(byte[] data) => Try.Run(() => CompressBytes(data), "Error compressing with Brotli.");

        private static byte[] CompressBytes(byte[] data)
        {
            using var ms = new MemoryStream();
            using var bs = new BrotliStream(ms, CompressionLevel.SmallestSize);
            bs.Write(data, 0, data.Length);
            bs.Flush();
            bs.Close();
            return ms.ToArray().ToResponse();
        }

        public Response<byte[]> Decompress(byte[] data) => Try.Run(() => DecompressBytes(data), "Error decompressing with Brotli.");

        private static byte[] DecompressBytes(byte[] data)
        {
            using var ms = new MemoryStream(data);
            using var bs = new BrotliStream(ms, CompressionMode.Decompress);
            using var cs = new MemoryStream();
            bs.CopyTo(cs);
            bs.Flush();
            bs.Close();
            return cs.ToArray().ToResponse();
        }
    }
}

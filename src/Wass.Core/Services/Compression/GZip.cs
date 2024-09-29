using ContainerExpressions.Containers;
using System.IO.Compression;

namespace Wass.Core.Services.Compression
{
    public interface IGZip
    {
        Response<byte[]> Compress(byte[] data);
        Response<byte[]> Decompress(byte[] data);
    }

    public sealed class GZip : IGZip
    {
        public Response<byte[]> Compress(byte[] data) => Try.Run(() => CompressBytes(data), "Error compressing with GZIP.");

        private static Response<byte[]> CompressBytes(byte[] data)
        {
            using var ms = new MemoryStream();
            using var zs = new GZipStream(ms, CompressionMode.Compress);
            zs.Write(data, 0, data.Length);
            zs.Flush();
            zs.Close();
            return ms.ToArray().ToResponse();
        }

        public Response<byte[]> Decompress(byte[] data) => Try.Run(() => DecompressBytes(data), "Error decompressing with GZIP.");

        private static Response<byte[]> DecompressBytes(byte[] data)
        {
            using var ms = new MemoryStream(data);
            using var zs = new GZipStream(ms, CompressionMode.Decompress);
            using var cs = new MemoryStream();
            zs.CopyTo(cs);
            zs.Flush();
            zs.Close();
            return cs.ToArray().ToResponse();
        }
    }
}

using System.IO.Compression;

namespace Wass.Core.Compression
{
    internal sealed class GZip
    {
        public byte[] Compress(byte[] data) => CompressStream(data);
        public byte[] Decompress(byte[] data) => DecompressStream(data);

        private static byte[] CompressStream(byte[] data)
        {
            using var ms = new MemoryStream();
            using var zs = new GZipStream(ms, CompressionMode.Compress);
            zs.Write(data, 0, data.Length);
            zs.Close();
            return ms.ToArray();
        }

        private static byte[] DecompressStream(byte[] data)
        {
            using var ms = new MemoryStream(data);
            using var zs = new GZipStream(ms, CompressionMode.Decompress);
            using var cs = new MemoryStream();
            zs.CopyTo(cs);
            return cs.ToArray();
        }
    }
}

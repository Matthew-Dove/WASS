using System.IO.Compression;

namespace Wass.Code.Compression
{
    internal static class GZip
    {
        public static byte[] Compress(byte[] data)
        {
            using var ms = new MemoryStream();
            using var zs = new GZipStream(ms, CompressionMode.Compress);
            zs.Write(data, 0, data.Length);
            zs.Close();
            return ms.ToArray();
        }

        public static byte[] Decompress(byte[] data)
        {
            using var ms = new MemoryStream(data);
            using var zs = new GZipStream(ms, CompressionMode.Decompress);
            using var cs = new MemoryStream();
            zs.CopyTo(cs);
            return cs.ToArray();
        }
    }
}

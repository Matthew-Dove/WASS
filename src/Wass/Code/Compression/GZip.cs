using System.IO.Compression;

namespace Wass.Code.Compression
{
    public interface IGZipClient
    {
        byte[] Compress(byte[] data);
        byte[] Decompress(byte[] data);
    }

    public sealed class GZipClient : IGZipClient
    {
        public byte[] Compress(byte[] data) => GZip.Compress(data);
        public byte[] Decompress(byte[] data) => GZip.Decompress(data);
    }

    public static class GZip
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

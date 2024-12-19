using ContainerExpressions.Containers;

namespace Wass.Core.Models.Options
{
    public class CompressionOptions : SmartEnum
    {
        public static readonly CompressionOptions Brotli = new();
        public static readonly CompressionOptions GZip = new();

        private CompressionOptions() { }
    }
}

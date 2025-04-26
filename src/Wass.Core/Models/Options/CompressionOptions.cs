using ContainerExpressions.Containers;

namespace Wass.Core.Models.Options
{
    public class CompressionOptions : SmartEnum
    {
        public static readonly CompressionOptions None = new(1);
        public static readonly CompressionOptions Brotli = new(2);
        public static readonly CompressionOptions GZip = new(3);

        private CompressionOptions(int value) : base(value) { }
    }
}

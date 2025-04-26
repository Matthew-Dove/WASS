using ContainerExpressions.Containers;
using Wass.Core.Models.Options;

namespace Wass.Core.Models
{
    public sealed class ActionRequest
    {
        public string Source { get; set; }
        public string File { get; set; }
        public string[] Tags { get; set; }
        public EnumRange<CompressionOptions> Compression { get; set; }
        public EnumRange<EncryptionOptions> Encryption { get; set; }
    }
}

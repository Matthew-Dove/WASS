using ContainerExpressions.Containers;
using Wass.Core.Models.Options;

namespace Wass.Core.Models
{
    public sealed class ActionRequest
    {
        public bool IsDryRun { get; set; }
        public string Source { get; set; }
        public string File { get; set; }
        public string[] Tags { get; set; }
        public EnumRange<CompressionOptions> Compression { get; set; }
        public EnumRange<EncryptionOptions> Encryption { get; set; }
        public string FileHash { get; set; }

        public ActionRequest()
        {
            IsDryRun = false;
            Source = string.Empty;
            File = string.Empty;
            Tags = Array.Empty<string>();
            Compression = SmartEnum<CompressionOptions>.FromObject(CompressionOptions.None);
            Encryption = SmartEnum<EncryptionOptions>.FromObject(EncryptionOptions.None);
            FileHash = string.Empty;
        }
    }
}

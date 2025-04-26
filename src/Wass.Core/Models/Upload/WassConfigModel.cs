namespace Wass.Core.Models.Upload
{
    public sealed class WassConfigModel
    {
        public string EncryptionKeyId { get; set; }
        public string HashKeyId { get; set; }
        public string Version { get; set; }
        public string Resource { get; set; }
        public string Encryption { get; set; }
        public string Compression { get; set; }
    }
}

namespace Wass.Core.Models.Upload
{
    /// <summary>
    /// This model is not going to be encrypted, it must be read in order to determine the encryption (etc) used for an upload (when restoring).
    /// <para>Therefore this model should not contain any sensative information.</para>
    /// </summary>
    public sealed class WassConfigModel
    {
        public string EncryptionKeyId { get; set; }
        public string HashKeyId { get; set; }
        public string Version { get; set; }
        public string Resource { get; set; }
        public string Encryption { get; set; }
        public string Compression { get; set; }
        public string TemplatePath { get; set; }
    }
}

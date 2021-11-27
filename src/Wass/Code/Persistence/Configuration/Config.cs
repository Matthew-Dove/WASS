namespace Wass.Code.Persistence.Configuration
{
    public static class Config
    {
        public static EncryptionConfig Encryption { get; set; } = new();
        public static S3Config S3 { get; set; } = new();
    }
}

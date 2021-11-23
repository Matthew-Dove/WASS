namespace Wass.Code.Persistence.Configuration
{
    public sealed class EncryptionConfig
    {
        public string Password { get; set; }
    }

    internal static class EncryptionConfigExtensions
    {
        public static bool IsValid(this EncryptionConfig config)
        {
            return
                config != null &&
                !string.IsNullOrEmpty(config.Password) &&
                config.Password.Length > 0;
        }
    }
}

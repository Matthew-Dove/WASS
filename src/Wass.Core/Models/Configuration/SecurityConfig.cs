namespace Wass.Core.Models.Configuration
{
    public sealed class SecurityConfig
    {
        public const string SECTION_NAME = "Security";

        /// <summary>Used for encryption.</summary>
        public string Password { get; set; }

        /// <summary>Used for hashing.</summary>
        public string Salt { get; set; }
    }

    public static class SecurityConfigExtensions
    {
        public static bool IsValid(this SecurityConfig config)
        {
            return
                config != null &&
                !string.IsNullOrEmpty(config.Password) &&
                !string.IsNullOrEmpty(config.Salt);
        }
    }
}

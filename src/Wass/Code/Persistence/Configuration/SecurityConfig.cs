namespace Wass.Code.Persistence.Configuration
{
    public sealed class SecurityConfig
    {
        /// <summary>Used for encryption.</summary>
        public string Password { get; set; }

        /// <summary>Used for hashing.</summary>
        public string Salt { get; set; }
    }

    internal static class SecurityConfigExtensions
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

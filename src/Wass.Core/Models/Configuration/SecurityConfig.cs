using Wass.Core.Services.Encryption;

namespace Wass.Core.Models.Configuration
{
    public sealed class SecurityConfig
    {
        public const string SECTION_NAME = "Security";

        /// <summary>An Id to mark the password that was used, so you know what the password was without having to store it in some way.</summary>
        public string PasswordKeyId { get; set; }

        /// <summary>Used for encryption.</summary>
        public string Password { get; set; }

        /// <summary>An Id to mark the salt that was used, so you know what the salt was without having to store it in some way.</summary>
        public string SaltKeyId { get; set; }

        /// <summary>Used for hashing.</summary>
        public string Salt { get; set; }
    }

    public static class SecurityConfigExtensions
    {
        /// <summary>
        /// Both the salt, and password are optional.
        /// <para>If the salt, or password have values; they are required to be of a minimum size.</para>
        /// <para>If the command is explicitly for, or has an option to encrypt / decrypt; then the password is required.</para>
        /// </summary>
        /// <param name="passwordRequired">Set to true for encryption, or decryption operations; otherwise false.</param>
        public static bool IsValid(this SecurityConfig config, bool passwordRequired)
        {
            const int minSaltSize = 32; // 16 bytes == 128 bits == 32 hex characters.
            var isValid = config != null;

            /**
             * For a 16 byte[] salt array (128 bits) to be represented as a string, there are the options:
             * - Hex (32): 842d2744dfaa08b411f291e202e6905c
             * - Base64 (24): hC0nRN+qCLQR8pHiAuaQXA==
             * 
             * UTF-8 encoding is not an option, as the cryto random byte values fall outside of the valid UTF-8 character range.
             * The base64 option could drop the padding, and be represented in 22 characters.
             * 
             * I'm going to go with the hex option at 32 (minimum) characters, as it's the most common format for binary (which the salt is expected to be).
             * The password can be assumed UTF-8 encoded, and if users prefer to use bytes; they can convert them to either hex; or base64 first.
            **/

            if (!string.IsNullOrEmpty(config?.Salt))
            {
                isValid = isValid && !string.IsNullOrWhiteSpace(config.SaltKeyId);
                isValid = isValid && (config.Salt.Length >= minSaltSize && config.Salt.IsValidHex());
            }
            if (!string.IsNullOrEmpty(config?.Password))
            {
                isValid = isValid && !string.IsNullOrWhiteSpace(config.PasswordKeyId);
                isValid = isValid && config.Password.Length >= C.OpSecSize;
            }
            if (passwordRequired)
            {
                isValid = isValid && !string.IsNullOrEmpty(config?.Password);
            }

            return isValid;
        }
    }
}

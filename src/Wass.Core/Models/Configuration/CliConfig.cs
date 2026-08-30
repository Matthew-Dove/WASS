namespace Wass.Core.Models.Configuration
{
    public sealed class CliConfig
    {
        public const string SECTION_NAME = "Cli";

        /// <summary>Hardcoded options to add to the cli input args array, before command parsing begins.</summary>
        public string Options { get; set; }
    }

    public static class UploadConfigExtensions
    {
        public static bool IsValid(this CliConfig config)
        {
            var isValid = config != null;

            isValid = isValid && config.Options != null; // Empty string is ok (preconfigured args are optional).

            return isValid;
        }
    }
}

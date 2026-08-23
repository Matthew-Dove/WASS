namespace Wass.Core.Models.Configuration
{
    public sealed class DownloadConfig
    {
        public const string SECTION_NAME = "Download";

        /// <summary>The local disk, or network root path to restore downloaded files to.</summary>
        public string LocalRootPath { get; set; }
    }

    public static class DownloadConfigExtensions
    {
        public static bool IsValid(this DownloadConfig config)
        {
            var isValid = config != null;

            isValid = isValid && !string.IsNullOrEmpty(config.LocalRootPath);
            isValid = isValid && Directory.Exists(config.LocalRootPath);

            return isValid;
        }
    }
}

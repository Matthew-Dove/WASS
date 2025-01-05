using System.Net;
using System.Text.RegularExpressions;

namespace Wass.Core.Models.Configuration
{
    public sealed class DestinationConfig
    {
        public const string SECTION_NAME = "Destination";

        public Dictionary<string, DestinationModel> Sources { get; set; }
    }

    public sealed class DestinationModel
    {
        public string AccessKeyId { get; set; }
        public string SecretAccessKey { get; set; }
        public string Bucket { get; set; }
        public string Region { get; set; }
        public string ServiceUrl { get; set; }
    }

    public static class DestinationExtensions
    {
        private const string _uppercase = @"[A-Z]";
        private const string _dashesAdjacentToPeriods = @"-\.|\.-";
        private const string _consecutivePeriods = @"\.\.";
        private const string _or = @"|";

        private static readonly string _invalidBucketName = string.Concat(_uppercase, _or, _dashesAdjacentToPeriods, _or, _consecutivePeriods);

        public static bool IsValid(this DestinationConfig config)
        {
            var isValid = config?.Sources != null && config.Sources.Count > 0;

            foreach (var source in config.Sources)
            {
                isValid = isValid && !string.IsNullOrEmpty(source.Value.AccessKeyId);
                isValid = isValid && !string.IsNullOrEmpty(source.Value.SecretAccessKey);
                isValid = isValid && source.Value.Bucket.IsBucketValid();
                isValid = isValid && !string.IsNullOrEmpty(source.Value.Region);
                isValid = isValid && (!string.IsNullOrEmpty(source.Value.ServiceUrl) && Uri.TryCreate(source.Value.ServiceUrl, UriKind.Absolute, out _));
            }

            return isValid;
        }

        /// <summary>Creates a random bucket name.</summary>
        public static string GenerateBucketName(this string _) => Path.GetRandomFileName().Replace(".", "").ToLower();

        /**
         * S3 Bucket Naming Requirements:
         * - The name must be between 3, and 63 characters long (inclusive), containing lower-case characters, numbers, periods, and dashes.
         * - The name must start with a lowercase letter or number, and cannot end with a dash, have consecutive periods, use dashes adjacent to periods, or be in the IP address format.
         * - The prefix "xn--", and the suffix "-s3alias" are reserved; for best compatibility it's recommend dots ".", are not included in bucket names.
        **/
        public static bool IsBucketValid(this string bucket)
        {
            return
                !string.IsNullOrEmpty(bucket) && // Must have a value.
                (bucket.Length >= 3 || bucket.Length <= 63) && // Must be between 3, and 63 (inclusive) characters in length.
                (char.IsNumber(bucket[0]) || char.IsLower(bucket[0])) && // Must start with a number, or a lowercase letter.
                bucket[^1] != '-' && // Cannot end with a dash.
                !IPAddress.TryParse(bucket, out _) && // Cannot be an IP address.
                !Regex.IsMatch(bucket, _invalidBucketName) && // Must be lowercase, optionaly with numbers, periods, and dashes.
                !bucket.StartsWith("xn--") && // Prefix is reserved.
                !bucket.EndsWith("-s3alias"); // Suffix is reserved.
        }
    }
}

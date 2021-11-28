using Amazon;
using System.Net;
using System.Text.RegularExpressions;

namespace Wass.Code.Persistence.Configuration
{
    public sealed class S3Config
    {
        public string AccessKeyId { get; set; }
        public string SecretAccessKey { get; set; }
        public string Region { get; set; } = "us-east-2"; // Ohio gets new features early (letting us-west-2 be the test bed), and is a cheap region; can be supplied per step.
        public string Bucket { get; set; } = Path.GetRandomFileName().Replace(".", "").ToLower(); // Optional: bucket can be supplied per step.
    }

    internal static class S3ConfigExtensions
    {
        private const string _uppercase = @"[A-Z]";
        private const string _dashesAdjacentToPeriods = @"-\.|\.-";
        private const string _consecutivePeriods = @"\.\.";
        private const string _or = @"|";

        private static readonly string _invalidBucketName = string.Concat(_uppercase, _or, _dashesAdjacentToPeriods, _or, _consecutivePeriods);
        private static readonly HashSet<string> _regions = new(RegionEndpoint.EnumerableAllRegions.Select(x => x.SystemName));

        public static bool IsValid(this S3Config config)
        {
            return
                config != null &&
                !string.IsNullOrEmpty(config.GetAccessKeyId()) &&
                !string.IsNullOrEmpty(config.GetSecretAccessKey()) &&
                !string.IsNullOrEmpty(config.Region) &&
                _regions.Contains(config.Region) &&
                IsBucketValid(config.Bucket);
        }

        public static string GetAccessKeyId(this S3Config config)
        {
            return
                string.IsNullOrEmpty(config?.AccessKeyId) ?
                Environment.GetEnvironmentVariable("WASS_AWS_S3_AccessKeyId", EnvironmentVariableTarget.User) :
                config.AccessKeyId;
        }

        public static string GetSecretAccessKey(this S3Config config)
        {
            return
                string.IsNullOrEmpty(config?.AccessKeyId) ?
                Environment.GetEnvironmentVariable("WASS_AWS_S3_SecretAccessKey", EnvironmentVariableTarget.User) :
                config.SecretAccessKey;
        }

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

using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using System.Net;
using System.Security.Cryptography;
using Wass.Code.Infrastructure;
using Wass.Code.Persistence.Configuration;

namespace Wass.Code.Persistence.Aws
{
    internal static class S3
    {
        private static readonly AmazonS3Client _client;

        static S3()
        {
            var config = Config.S3;
            if (!config.IsValid())
            {
                Log.Error("AWS S3 config is not valid.");
                throw new InvalidOperationException("AWS S3 config is not valid.");
            }
            _client = new(config.AccessKeyId, config.SecretAccessKey, new AmazonS3Config { RegionEndpoint = RegionEndpoint.GetBySystemName(config.Region) });
        }

        public static async Task<bool> Upload(string bucket, string key, byte[] data, S3StorageClass storageClass)
        {
            using var ms = new MemoryStream(data);
            var md5Hash = GetMD5Hash(ms);
            ms.Position = 0;

            var request = new PutObjectRequest
            {
                InputStream = ms,
                BucketName = bucket,
                Key = key,
                StorageClass = storageClass,
                BucketKeyEnabled = true,
                MD5Digest = md5Hash,
                StreamTransferProgress = (_, e) => Log.Trace($"S3 file transfer progress for {key}: {e.PercentDone}%.")
            };

            var result = (await _client.PutObjectAsync(request)).Trail(x => $"Upload status to S3 for {key}: {x.HttpStatusCode}.");
            return result.HttpStatusCode == HttpStatusCode.OK;
        }

        private static string GetMD5Hash(Stream stream)
        {
            using var md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(stream);
            return Convert.ToBase64String(hash);
        }
    }
}

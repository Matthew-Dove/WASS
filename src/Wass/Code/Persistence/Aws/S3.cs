using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using System.Collections.Concurrent;
using System.Net;
using System.Security.Cryptography;
using Wass.Code.Infrastructure;
using Wass.Code.Persistence.Configuration;

namespace Wass.Code.Persistence.Aws
{
    internal static class S3
    {
        private static readonly AmazonS3Client _client;
        private static readonly ConcurrentDictionary<string, bool> _doesBucketExist = new();

        static S3()
        {
            var config = Config.S3;
            if (!config.IsValid())
            {
                Log.Error("AWS S3 config is not valid.");
                throw new InvalidOperationException("AWS S3 config is not valid.");
            }
            _client = new(config.GetAccessKeyId(), config.GetSecretAccessKey(), new AmazonS3Config { RegionEndpoint = RegionEndpoint.GetBySystemName(config.Region) });
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
                ObjectLockMode = default,
                ObjectLockRetainUntilDate = default,
                StreamTransferProgress = (_, e) => Log.Trace($"S3 file transfer progress for [{key}]: {e.PercentDone}%.")
            };

            var result = (await _client.PutObjectAsync(request)).Trail(x => $"Upload status to S3 for [{key}]: {x.HttpStatusCode}.");
            return result.HttpStatusCode == HttpStatusCode.OK;
        }

        public static async ValueTask<bool> DoesFileExist(string bucket, string key)
        {
            var fileExists = false;

            try
            {
                var result = await _client.GetObjectMetadataAsync(bucket, key);
                if (result.HttpStatusCode == HttpStatusCode.OK)
                {
                    fileExists = false.Trail($"The file [{key}] was found in the bucket [{bucket}].");
                }
            }
            catch (AmazonS3Exception aex) when (aex.StatusCode == HttpStatusCode.NotFound)
            {
                fileExists = false.Trail($"The key [{key}], does not exist in the bucket [{bucket}].");
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(DoesFileExist));
            }

            return fileExists;
        }

        public static async ValueTask<bool> DoesBucketExist(string bucket)
        {
            if (_doesBucketExist.ContainsKey(bucket)) { _doesBucketExist.TryGetValue(bucket, out var doesExist); return doesExist; }

            if (await AmazonS3Util.DoesS3BucketExistV2Async(_client, bucket).Trail(x => $"Does AWS S3 bucket {bucket} exist: {x}."))
            {
                _doesBucketExist.TryAdd(bucket, true);
                return true;
            }

            return false;
        }

        public static async ValueTask<bool> CreateBucket(string bucket)
        {
            var result = await _client.PutBucketAsync(new PutBucketRequest { BucketName = bucket });

            if (result.HttpStatusCode == HttpStatusCode.OK.Trail(x => $"Creating new bucket in S3 [{bucket}], status: {x}."))
            {
                _doesBucketExist.TryAdd(bucket, true);
                return true;
            }

            return false;
        }

        private static string GetMD5Hash(Stream stream)
        {
            using var md5 = MD5.Create();
            var hash = md5.ComputeHash(stream);
            return Convert.ToBase64String(hash);
        }
    }
}

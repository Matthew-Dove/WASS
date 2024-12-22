using Amazon.S3.Model;
using Amazon.S3.Util;
using Amazon.S3;
using System.Collections.Concurrent;
using System.Net;
using System.Security.Cryptography;
using ContainerExpressions.Containers;
using Microsoft.Extensions.Options;
using Wass.Core.Models.Configuration;
using Amazon;

namespace Wass.Core.Services.Persistence
{
    public interface IS3
    {
        Task<Response<bool>> DoesFileExist(string bucket, string key);
        Task<Response<Unit>> CreateFile(string bucket, string key, byte[] data, S3StorageClass storageClass);
        ValueTask<Response<bool>> DoesBucketExist(string bucket);
        Task<Response<Unit>> CreateBucket(string bucket);
    }

    public sealed class S3 : IS3
    {
        private static readonly object _lock = new object();
        private static AmazonS3Client _client;
        private static readonly ConcurrentDictionary<string, bool> _doesBucketExist = new();

        public S3(IOptions<S3Config> config)
        {
            if (!config.Value.IsValid()) new InvalidOperationException("AWS S3 config is not valid.").LogError().ThrowError();
            _client = GetClient(config.Value);
        }

        private static AmazonS3Client GetClient(S3Config config)
        {
            if (_client == null)
            {
                lock (_lock)
                {
                    if (_client == null)
                    {
                        var awsConfig = new AmazonS3Config { RegionEndpoint = RegionEndpoint.GetBySystemName(config.Region) };
                        if (config.ServiceUrl != null)
                        {
                            awsConfig.ServiceURL = config.ServiceUrl;
                            awsConfig.AuthenticationRegion = config.Region;
                            awsConfig.ForcePathStyle = true;
                        }
                        _client = new(config.GetAccessKeyId(), config.GetSecretAccessKey(), awsConfig);
                    }
                }
            }
            return _client;
        }

        public async Task<Response<bool>> DoesFileExist(string bucket, string key)
        {
            return await DoesFileExistInS3(bucket, key);
        }

        private static async ResponseAsync<bool> DoesFileExistInS3(string bucket, string key)
        {
            var fileExists = false;

            try
            {
                var result = await _client.GetObjectMetadataAsync(bucket, key);
                if (result.HttpStatusCode == HttpStatusCode.OK)
                {
                    fileExists = true.LogValue("The file [{Key}] was found in the bucket [{Bucket}].".WithArgs(key, bucket));
                }
            }
            catch (AmazonS3Exception aex) when (aex.StatusCode == HttpStatusCode.NotFound)
            {
                Log.Info("The key [{Key}], does not exist in the bucket [{Bucket}].".WithArgs(key, bucket));
            }
            catch (AggregateException ae) when (ae.InnerExceptions.Count == 1 && ae.InnerException is AmazonS3Exception aex && aex.StatusCode == HttpStatusCode.NotFound)
            {
                Log.Info("The key [{Key}], does not exist in the bucket [{Bucket}].".WithArgs(key, bucket));
            }

            return fileExists;
        }

        public async Task<Response<Unit>> CreateFile(string bucket, string key, byte[] data, S3StorageClass storageClass)
        {
            var response = await CreateFileInS3(bucket, key, data, storageClass);
            return response.Validate(x => x == HttpStatusCode.OK).Transform(_ => Unit.Instance);
        }

        private static async ResponseAsync<HttpStatusCode> CreateFileInS3(string bucket, string key, byte[] data, S3StorageClass storageClass)
        {
            using var ms = new MemoryStream(data);
            var md5Hash = GetMD5Hash(ms);
            ms.Position = 0;
            var sha256Hash = GetSHA256Hash(ms);
            ms.Position = 0;

            var request = new PutObjectRequest
            {
                BucketName = bucket,
                InputStream = ms, // Max size for an object put request is 5GB , use the "multipart upload api" for upto 5TB in size.
                Key = key, // Case sensitive.
                StorageClass = storageClass, // Different storage classes provide varying levels of durability, availability, and cost.
                BucketKeyEnabled = true, // Encrypt objects at rest.
                ObjectLockMode = ObjectLockMode.Governance, // Bucket versioning must be enabled to lock objects.
                ObjectLockRetainUntilDate = DateTime.UtcNow.AddYears(99), // 100 years is the max retention period for an object lock.
                IfNoneMatch = "*", // Multipart upload could potentially fail here - only the first upload might work.
                MD5Digest = md5Hash, // Ensure the content is transmitted, and stored correctly.
                ChecksumAlgorithm = ChecksumAlgorithm.SHA256,
                ChecksumSHA256 = sha256Hash,
                StreamTransferProgress = (_, e) => e.LogValue(x => "S3 file transfer progress for [{Key}]: {PercentDone}%.".WithArgs(key, x.PercentDone)),
            };

            var result = await _client.PutObjectAsync(request).LogValueAsync(x => "Upload status to S3 for [{Key}]: {HttpStatusCode}.".WithArgs(key, x.HttpStatusCode));
            return result.HttpStatusCode;
        }

        private static string GetMD5Hash(Stream stream)
        {
            using var md5 = MD5.Create();
            var hash = md5.ComputeHash(stream);
            return Convert.ToBase64String(hash);
        }

        private static string GetSHA256Hash(Stream stream)
        {
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(stream);
            return Convert.ToBase64String(hash);
        }

        public async ValueTask<Response<bool>> DoesBucketExist(string bucket)
        {
            if (_doesBucketExist.ContainsKey(bucket)) return Response.Create(true);
            return await DoesBucketExistInS3(bucket);
        }

        private static async ResponseAsync<bool> DoesBucketExistInS3(string bucket)
        {
            var bucketExists = await AmazonS3Util.DoesS3BucketExistV2Async(_client, bucket).LogValueAsync(x => "Does AWS S3 bucket [{Bucket}] exist: {Exists}.".WithArgs(bucket, x));

            if (bucketExists)
            {
                try
                {
                    // The following bucket configuration checks are for logging only, as the user may choose to create custom bucket configs after it's been created.
                    var version = await _client.GetBucketVersioningAsync(bucket).LogValueAsync(x => "Bucket [{Bucket}] http status: {HttpStatusCode}, versioning status: {VersioningStatus}.".WithArgs(bucket, x.HttpStatusCode, x.VersioningConfig?.Status));
                    var encryption = await _client.GetBucketEncryptionAsync(new GetBucketEncryptionRequest { BucketName = bucket }).LogValueAsync(x => "Bucket [{Bucket}] http status: {HttpStatusCode}, encryption rule(s) found: {RuleCount}. {Rules}".WithArgs(bucket, x.HttpStatusCode, x.ServerSideEncryptionConfiguration.ServerSideEncryptionRules.Count, string.Join(". ", x.ServerSideEncryptionConfiguration.ServerSideEncryptionRules.Select(x => $"BucketKeyEnabled: {x.BucketKeyEnabled}, ServerSideEncryptionAlgorithm: {x.ServerSideEncryptionByDefault.ServerSideEncryptionAlgorithm.Value}, KeyId: {x.ServerSideEncryptionByDefault.ServerSideEncryptionKeyManagementServiceKeyId}."))));
                    var objectLock = await _client.GetObjectLockConfigurationAsync(new GetObjectLockConfigurationRequest { BucketName = bucket }).LogValueAsync(x => "Bucket [{Bucket}] http status: {HttpStatusCode}, object lock: {ObjectLockEnabled}. Retention mode: {RetentionMode}, years: {RetentionYears}, days: {RetentionDays}.".WithArgs(bucket, x.HttpStatusCode, x.ObjectLockConfiguration.ObjectLockEnabled, x.ObjectLockConfiguration.Rule?.DefaultRetention?.Mode?.Value, x.ObjectLockConfiguration.Rule?.DefaultRetention?.Years, x.ObjectLockConfiguration.Rule?.DefaultRetention?.Days));
                }
                catch (AmazonS3Exception aex) when (aex.StatusCode == HttpStatusCode.NotFound)
                {
                    Log.Info("Bucket [{Bucket}] does not have Object Lock configured.".WithArgs(bucket));
                }
                catch (AmazonS3Exception aex) when (aex.StatusCode == HttpStatusCode.Moved)
                {
                    Log.Error("Bucket [{Bucket}] exists in a different region, update the destination's config to the correct region.".WithArgs(bucket));
                    throw;
                }
                catch (AggregateException ae) when (ae.InnerExceptions.Count == 1 && ae.InnerException is AmazonS3Exception aex && (aex.StatusCode == HttpStatusCode.NotFound || aex.StatusCode == HttpStatusCode.Moved))
                {
                    if (aex.StatusCode == HttpStatusCode.NotFound) { Log.Info("Bucket [{Bucket}] does not have Object Lock configured.".WithArgs(bucket)); }
                    if (aex.StatusCode == HttpStatusCode.Moved)
                    {
                        Log.Error("Bucket [{Bucket}] exists in a different region, update the destination's config to the correct region.".WithArgs(bucket));
                        throw;
                    }
                }

                _doesBucketExist.TryAdd(bucket, true);
            }

            return bucketExists;
        }

        public async Task<Response<Unit>> CreateBucket(string bucket)
        {
            var response = await CreateBucketInS3(bucket);
            return response.Validate(Lambda.Identity).Transform(_ => Unit.Instance);
        }

        // This method is idempotent, if any of these step fails, you can run it again to recover.
        private static async ResponseAsync<bool> CreateBucketInS3(string bucket)
        {
            var bucketExists = false;

            try
            {
                var create = await _client.PutBucketAsync(new PutBucketRequest { BucketName = bucket, ObjectLockEnabledForBucket = true });
                bucketExists = HttpStatusCode.OK == create.HttpStatusCode.LogValue(x => "Creating new bucket in S3 [{Bucket}], status: {HttpStatusCode}.".WithArgs(bucket, x));
            }
            catch (AmazonS3Exception aex) when (aex.StatusCode == HttpStatusCode.Conflict)
            {
                bucketExists = true.LogValue("The S3 bucket [{Bucket}] already exists.".WithArgs(bucket));
            }
            catch (AggregateException ae) when (ae.InnerExceptions.Count == 1 && ae.InnerException is AmazonS3Exception aex && aex.StatusCode == HttpStatusCode.Conflict)
            {
                bucketExists = true.LogValue("The S3 bucket [{Bucket}] already exists.".WithArgs(bucket));
            }

            if (bucketExists)
            {
                var versioningRequest = new PutBucketVersioningRequest { BucketName = bucket, VersioningConfig = new S3BucketVersioningConfig { Status = VersionStatus.Enabled } };
                var versioningResult = await _client.PutBucketVersioningAsync(versioningRequest).LogValueAsync(x => "Adding versioning to bucket [{Bucket}] result: {HttpStatusCode}.".WithArgs(bucket, x.HttpStatusCode));
                bucketExists = versioningResult.HttpStatusCode == HttpStatusCode.OK;
            }

            if (bucketExists)
            {
                var encryptionRequest = new ServerSideEncryptionConfiguration { ServerSideEncryptionRules = new List<ServerSideEncryptionRule> { new ServerSideEncryptionRule {
                            BucketKeyEnabled = true,
                            ServerSideEncryptionByDefault = new ServerSideEncryptionByDefault { ServerSideEncryptionAlgorithm = ServerSideEncryptionMethod.AES256  }
                        }
                    }
                };
                var encryptionResult = await _client.PutBucketEncryptionAsync(new PutBucketEncryptionRequest { BucketName = bucket, ServerSideEncryptionConfiguration = encryptionRequest }).LogValueAsync(x => "Adding encryption to bucket [{Bucket}] result: {HttpStatusCode}.".WithArgs(bucket, x.HttpStatusCode));
                bucketExists = encryptionResult.HttpStatusCode == HttpStatusCode.OK;
            }

            if (bucketExists)
            {
                var lockRequest = new ObjectLockConfiguration
                {
                    ObjectLockEnabled = ObjectLockEnabled.Enabled, Rule = new ObjectLockRule { DefaultRetention = new DefaultRetention {
                            Mode = ObjectLockRetentionMode.Governance,
                            Years = 99
                        }
                    }
                };
                var lockResult = await _client.PutObjectLockConfigurationAsync(new PutObjectLockConfigurationRequest { BucketName = bucket, ObjectLockConfiguration = lockRequest });
                bucketExists = lockResult.HttpStatusCode == HttpStatusCode.OK;
            }

            if (bucketExists) _doesBucketExist.TryAdd(bucket, true);
            return bucketExists;
        }
    }
}

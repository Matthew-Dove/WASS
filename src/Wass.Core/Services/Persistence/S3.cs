using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using ContainerExpressions.Containers;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Net;
using System.Security.Cryptography;
using Wass.Core.Models.Configuration;

namespace Wass.Core.Services.Persistence
{
    public interface IS3
    {
        Task<Response<bool>> DoesFileExist(string source, string bucket, string key);
        Task<Response<Unit>> CreateFile(string source, string bucket, string key, byte[] data, S3StorageClass storageClass);
        ValueTask<Response<bool>> DoesBucketExist(string source, string bucket);
        Task<Response<Unit>> CreateBucket(string source, string bucket);
        Task<Response<string[]>> ListFiles (string source, string bucket, string prefix);
        Task<Response<byte[]>> DownloadFile(string source, string bucket, string key);
    }

    public sealed class S3 : IS3
    {
        private const int _daysToWaitforMultipartUploads = 7;
        private const string _removeExpiredDeleteMarkersRuleId = "WassRemoveExpiredDeleteMarkers";
        private static readonly string _abortMultipartUploadsRuleId = $"WassAbortIncompleteMultipartUploadAfter{_daysToWaitforMultipartUploads}Days";

        private static readonly ConcurrentDictionary<string, AmazonS3Client> _clients = new(StringComparer.OrdinalIgnoreCase);
        private static readonly ConcurrentDictionary<string, bool> _doesBucketExist = new();

        private readonly IOptions<DestinationConfig> _config;

        public S3(IOptions<DestinationConfig> config)
        {
            _config = config;
        }

        private static AmazonS3Client GetClient(DestinationConfig config, string source)
        {
            return _clients.GetOrAdd(source, src =>
            {
                config.ThrowIf(x => !x.IsValid(), "Destination config is not valid.");
                var destination = config.Sources[src];
                var awsConfig = new AmazonS3Config { ServiceURL = destination.ServiceUrl, AuthenticationRegion = destination.Region, ForcePathStyle = true };
                return new AmazonS3Client(destination.AccessKeyId, destination.SecretAccessKey, awsConfig);
            });
        }

        public async Task<Response<bool>> DoesFileExist(string source, string bucket, string key)
        {
            var client = GetClient(_config.Value, source);
            return await DoesFileExistInS3(client, bucket, key);
        }

        private static async ResponseAsync<bool> DoesFileExistInS3(AmazonS3Client client, string bucket, string key)
        {
            var fileExists = false;

            try
            {
                var result = await client.GetObjectMetadataAsync(bucket, key);
                result.HttpStatusCode.ThrowIf(static x => x != HttpStatusCode.OK);
                fileExists = true.LogValue("The file [{Key}] was found in the bucket [{Bucket}].".WithArgs(key, bucket));
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

        public async Task<Response<Unit>> CreateFile(string source, string bucket, string key, byte[] data, S3StorageClass storageClass)
        {
            var client = GetClient(_config.Value, source);
            var response = await CreateFileInS3(client, bucket, key, data, storageClass);
            return response.Validate(x => x == HttpStatusCode.OK).Transform(_ => Unit.Instance);
        }

        private static async ResponseAsync<HttpStatusCode> CreateFileInS3(AmazonS3Client client, string bucket, string key, byte[] data, S3StorageClass storageClass)
        {
            using var ms = new MemoryStream(data);
            var md5Hash = GetMD5Hash(ms);
            ms.Position = 0;
            var sha256Hash = GetSHA256Hash(ms);
            ms.Position = 0;

            var request = new PutObjectRequest
            {
                BucketName = bucket,
                InputStream = ms, // Max size for an object put request is 5GB, use the "multipart upload api" for upto 5TB in size.
                Key = key, // Case sensitive.
                StorageClass = storageClass, // Different storage classes provide varying levels of durability, availability, and cost.
                BucketKeyEnabled = true, // Encrypt objects at rest.
                ObjectLockMode = ObjectLockMode.Governance, // Bucket versioning must be enabled to lock objects.
                ObjectLockRetainUntilDate = DateTime.UtcNow.AddYears(99), // 100 years is the max retention period for an object lock.
                IfNoneMatch = "*", // Multipart upload could potentially fail here - only the first upload might work.
                MD5Digest = md5Hash, // Ensure the content is transmitted, and stored correctly.
                ChecksumAlgorithm = ChecksumAlgorithm.SHA256,
                ChecksumSHA256 = sha256Hash, // Data integrity check ensuring the object written to S3 matches the local one.
                CannedACL = S3CannedACL.Private, // The object's access rights.
                StreamTransferProgress = (_, e) => e.LogValue(x => "S3 file transfer progress for [{Key}]: {PercentDone}%.".WithArgs(key, x.PercentDone)),
            };

            var result = await client.PutObjectAsync(request).LogValueAsync(x => "Upload status to S3 for [{Key}]: {HttpStatusCode}.".WithArgs(key, x.HttpStatusCode));
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

        public async ValueTask<Response<bool>> DoesBucketExist(string source, string bucket)
        {
            if (_doesBucketExist.ContainsKey($"{source}:{bucket}")) return Response.Create(true);
            var client = GetClient(_config.Value, source);
            return await DoesBucketExistInS3(client, source, bucket);
        }

        private static async ResponseAsync<bool> DoesBucketExistInS3(AmazonS3Client client, string source, string bucket)
        {
            var bucketExists = await AmazonS3Util.DoesS3BucketExistV2Async(client, bucket).LogValueAsync(x => "Does AWS S3 bucket [{Bucket}] exist: {Exists}.".WithArgs(bucket, x));

            if (bucketExists)
            {
                try
                {
                    // The following bucket configuration checks are for logging only, as the user may choose to create custom bucket configs after it's been created.
                    var lifecycle = await client.GetLifecycleConfigurationAsync(new GetLifecycleConfigurationRequest { BucketName = bucket }).LogValueAsync(x => "Bucket [{Bucket}] http status: {HttpStatusCode}, lifecycle rule(s) found: {RuleCount}. {Rules}.".WithArgs(bucket, x.HttpStatusCode, x.Configuration.Rules.Count, string.Join("; ", x.Configuration.Rules.Select(y => $"Id: {y.Id}, Status: {y.Status}"))));
                    var version = await client.GetBucketVersioningAsync(bucket).LogValueAsync(x => "Bucket [{Bucket}] http status: {HttpStatusCode}, versioning status: {VersioningStatus}.".WithArgs(bucket, x.HttpStatusCode, x.VersioningConfig?.Status));
                    var encryption = await client.GetBucketEncryptionAsync(new GetBucketEncryptionRequest { BucketName = bucket }).LogValueAsync(x => "Bucket [{Bucket}] http status: {HttpStatusCode}, encryption rule(s) found: {RuleCount}. {Rules}".WithArgs(bucket, x.HttpStatusCode, x.ServerSideEncryptionConfiguration.ServerSideEncryptionRules.Count, string.Join(". ", x.ServerSideEncryptionConfiguration.ServerSideEncryptionRules.Select(x => $"BucketKeyEnabled: {x.BucketKeyEnabled}, ServerSideEncryptionAlgorithm: {x.ServerSideEncryptionByDefault.ServerSideEncryptionAlgorithm.Value}, KeyId: {x.ServerSideEncryptionByDefault.ServerSideEncryptionKeyManagementServiceKeyId}."))));
                    var objectLock = await client.GetObjectLockConfigurationAsync(new GetObjectLockConfigurationRequest { BucketName = bucket }).LogValueAsync(x => "Bucket [{Bucket}] http status: {HttpStatusCode}, object lock: {ObjectLockEnabled}. Retention mode: {RetentionMode}, years: {RetentionYears}, days: {RetentionDays}.".WithArgs(bucket, x.HttpStatusCode, x.ObjectLockConfiguration.ObjectLockEnabled, x.ObjectLockConfiguration.Rule?.DefaultRetention?.Mode?.Value, x.ObjectLockConfiguration.Rule?.DefaultRetention?.Years, x.ObjectLockConfiguration.Rule?.DefaultRetention?.Days));
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

                _doesBucketExist.TryAdd($"{source}:{bucket}", true);
            }

            return bucketExists;
        }

        public async Task<Response<Unit>> CreateBucket(string source, string bucket)
        {
            var client = GetClient(_config.Value, source);
            var response = await CreateBucketInS3(client, source, bucket);
            return response.Validate(Lambda.Identity).Transform(_ => Unit.Instance);
        }

        // This method is idempotent, if any of these step fails, you can run it again to recover.
        private static async ResponseAsync<bool> CreateBucketInS3(AmazonS3Client client, string source, string bucket)
        {
            var bucketExists = false;

            try
            {
                var create = await client.PutBucketAsync(new PutBucketRequest { BucketName = bucket, ObjectLockEnabledForBucket = true, CannedACL = S3CannedACL.Private });
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
                var versioningResult = await client.PutBucketVersioningAsync(versioningRequest).LogValueAsync(x => "Adding versioning to bucket [{Bucket}] result: {HttpStatusCode}.".WithArgs(bucket, x.HttpStatusCode));
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
                var encryptionResult = await client.PutBucketEncryptionAsync(new PutBucketEncryptionRequest { BucketName = bucket, ServerSideEncryptionConfiguration = encryptionRequest }).LogValueAsync(x => "Adding encryption to bucket [{Bucket}] result: {HttpStatusCode}.".WithArgs(bucket, x.HttpStatusCode));
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
                var lockResult = await client.PutObjectLockConfigurationAsync(new PutObjectLockConfigurationRequest { BucketName = bucket, ObjectLockConfiguration = lockRequest }).LogValueAsync(x => "Adding governance retention to bucket [{Bucket}] result: {HttpStatusCode}.".WithArgs(bucket, x.HttpStatusCode));
                bucketExists = lockResult.HttpStatusCode == HttpStatusCode.OK;
            }

            if (bucketExists)
            {
                var multipartRequest = new LifecycleConfiguration { Rules = new List<LifecycleRule> {
                    new LifecycleRule {
                        Id = _abortMultipartUploadsRuleId,
                        Status = LifecycleRuleStatus.Enabled,
                        Filter = new LifecycleFilter(),
                        AbortIncompleteMultipartUpload = new LifecycleRuleAbortIncompleteMultipartUpload { DaysAfterInitiation = _daysToWaitforMultipartUploads },
                    },
                    new LifecycleRule {
                        Id = _removeExpiredDeleteMarkersRuleId,
                        Status = LifecycleRuleStatus.Enabled,
                        Filter = new LifecycleFilter(),
                        Expiration = new LifecycleRuleExpiration { ExpiredObjectDeleteMarker = true }
                    }
                }};

                var multipartResult = await client.PutLifecycleConfigurationAsync(new PutLifecycleConfigurationRequest { BucketName = bucket, Configuration = multipartRequest }).LogValueAsync(x => "Adding lifecycle rules: {RuleId} to bucket [{Bucket}] result: {HttpStatusCode}.".WithArgs($"{_abortMultipartUploadsRuleId}, and {_removeExpiredDeleteMarkersRuleId}", bucket, x.HttpStatusCode));
                bucketExists = multipartResult.HttpStatusCode == HttpStatusCode.OK;
            }

            if (bucketExists) _doesBucketExist.TryAdd($"{source}:{bucket}", true);
            return bucketExists;
        }

        public async Task<Response<string[]>> ListFiles(string source, string bucket, string prefix)
        {
            var client = GetClient(_config.Value, source);
            return await ListFilesInS3(client, bucket, prefix);
        }

        private static async ResponseAsync<string[]> ListFilesInS3(AmazonS3Client client, string bucket, string prefix)
        {
            var continuationToken = default(string);
            var files = new List<string>();
            var i = 0;

            do {
                var request = new ListObjectsV2Request { BucketName = bucket, Prefix = prefix, ContinuationToken = continuationToken };
                var result = await client.ListObjectsV2Async(request).LogValueAsync(x => "Listing keys for bucket [{Bucket}] result: {HttpStatusCode}, using prefix: \"{Prefix}\". Keys found: {KeyCount}, is key count truncated: {IsTruncated}, iteration: {PassCount}.".WithArgs(bucket, x.HttpStatusCode, prefix, x.KeyCount, x.IsTruncated, ++i));
                result.HttpStatusCode.ThrowIf(static x => x != HttpStatusCode.OK);
                continuationToken = result.NextContinuationToken;
                var keys = result.S3Objects.Select(x => x.Key);
                files.AddRange(keys);
            }
            while (continuationToken != null);

            return files.ToArray();
        }

        public async Task<Response<byte[]>> DownloadFile(string source, string bucket, string key)
        {
            var client = GetClient(_config.Value, source);
            return await DownloadFileFromS3(client, bucket, key);
        }

        private static async ResponseAsync<byte[]> DownloadFileFromS3(AmazonS3Client client, string bucket, string key)
        {
            var file = Array.Empty<byte>();

            try
            {
                var request = new GetObjectRequest { BucketName = bucket, Key = key };
                var response = await client.GetObjectAsync(request).LogValueAsync(x => "Download status from S3 for [{Key}]: {HttpStatusCode}.".WithArgs(key, x.HttpStatusCode));
                response.HttpStatusCode.ThrowIf(static x => x != HttpStatusCode.OK);

                var totalBytes = response.ContentLength;
                using var responseStream = response.ResponseStream;

                if (totalBytes > 0 && totalBytes <= int.MaxValue)
                {
                    file = new byte[(int)totalBytes];

                    var totalRead = 0;
                    var lastPercentDone = -1;

                    while (totalRead < totalBytes)
                    {
                        var chunkSize = Math.Min(81920, (int)totalBytes - totalRead);
                        var read = await responseStream.ReadAsync(file.AsMemory(totalRead, chunkSize));
                        if (read == 0) break; // End of stream.

                        totalRead += read;
                        var percentDone = (int)((totalRead * 100L) / totalBytes);
                        if (percentDone > lastPercentDone)
                        {
                            lastPercentDone = percentDone;
                            Log.Info("S3 file transfer progress for [{Key}]: {PercentDone}%.".WithArgs(key, percentDone));
                        }
                    }

                    // Shrink the array to what was actually downloaded.
                    if (totalRead < file.Length)
                    {
                        Array.Resize(ref file, totalRead);
                    }
                }
                else
                {
                    // Fallback when content length is unknown.
                    using var ms = new MemoryStream();
                    await responseStream.CopyToAsync(ms);
                    file = ms.ToArray();
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

            return file;
        }

        //private static async ResponseAsync<byte[]> DownloadFileFromS3(AmazonS3Client client, string bucket, string key)
        //{
        //    var file = Array.Empty<byte>();

        //    try
        //    {
        //        var request = new GetObjectRequest { BucketName = bucket, Key = key };
        //        var response = await client.GetObjectAsync(request).LogValueAsync(x => "Download status from S3 for [{Key}]: {HttpStatusCode}.".WithArgs(key, x.HttpStatusCode));
        //        response.HttpStatusCode.ThrowIf(static x => x != HttpStatusCode.OK);

        //        using var ms = new MemoryStream();
        //        await response.ResponseStream.CopyToAsync(ms);
        //        file = ms.ToArray();
        //    }
        //    catch (AmazonS3Exception aex) when (aex.StatusCode == HttpStatusCode.NotFound)
        //    {
        //        Log.Info("The key [{Key}], does not exist in the bucket [{Bucket}].".WithArgs(key, bucket));
        //    }
        //    catch (AggregateException ae) when (ae.InnerExceptions.Count == 1 && ae.InnerException is AmazonS3Exception aex && aex.StatusCode == HttpStatusCode.NotFound)
        //    {
        //        Log.Info("The key [{Key}], does not exist in the bucket [{Bucket}].".WithArgs(key, bucket));
        //    }

        //    return file;
        //}
    }
}

using Amazon.S3;
using ContainerExpressions.Containers;
using System.Collections.Concurrent;
using System.Net;

namespace Wass.Core.Services.Persistence
{
    public sealed class S3Sandbox : IS3
    {
        private const string _bucketPrefix = "bucket";
        private const string _keyPrefix = "key";

        private static readonly ConcurrentDictionary<string, byte[]> _s3 = new();

        private static readonly Task<Response<Unit>> _unitSuccess = Task.FromResult(Unit.ResponseSuccess);
        private static readonly Task<Response<Unit>> _unitError = Task.FromResult(Unit.ResponseError);
        private static readonly Task<Response<bool>> _true = Task.FromResult(Response.Create(true));
        private static readonly Task<Response<bool>> _false = Task.FromResult(Response.Create(false));

        public ValueTask<Response<bool>> DoesBucketExist(string source, string bucket)
        {
            if (_s3.ContainsKey($"{_bucketPrefix}:{source}:{bucket}")) return ValueTask.FromResult(Response.Create(true));
            return ValueTask.FromResult(Response.Create(false)).LogValue("Does AWS S3 bucket [{Bucket}] exist: {Exists}.".WithArgs(bucket, false));
        }

        public Task<Response<Unit>> CreateBucket(string source, string bucket)
        {
            if (_s3.ContainsKey($"{_bucketPrefix}:{source}:{bucket}")) return _unitSuccess.LogValue("The S3 bucket [{Bucket}] already exists.".WithArgs(bucket));
            _s3.TryAdd($"{_bucketPrefix}:{source}:{bucket}", Array.Empty<byte>());
            return _unitSuccess.LogValue("Creating new bucket in S3 [{Bucket}], status: {HttpStatusCode}.".WithArgs(bucket, HttpStatusCode.OK));
        }

        public Task<Response<bool>> DoesFileExist(string source, string bucket, string key)
        {
            if (_s3.ContainsKey($"{_keyPrefix}:{source}:{bucket}:{key}")) return _true.LogValue("The file [{Key}] was found in the bucket [{Bucket}].".WithArgs(key, bucket));
            return _false.LogValue("The key [{Key}], does not exist in the bucket [{Bucket}].".WithArgs(key, bucket));
        }

        public Task<Response<Unit>> CreateFile(string source, string bucket, string key, byte[] data, S3StorageClass storageClass)
        {
            Log.Info("S3 file transfer progress for [{Key}]: {PercentDone}%.".WithArgs(key, 100));
            if (_s3.ContainsKey($"{_keyPrefix}:{source}:{bucket}:{key}")) return _unitError.LogErrorValue("At least one of the pre-conditions you specified did not hold.");
            _s3.TryAdd($"{_keyPrefix}:{source}:{bucket}:{key}", data);
            return _unitSuccess.LogValue("Upload status to S3 for [{Key}]: {HttpStatusCode}.".WithArgs(key, HttpStatusCode.OK));
        }

        public Task<Response<string[]>> ListFiles(string source, string bucket, string prefix)
        {
            var prefixSearch = $"{_keyPrefix}:{source}:{bucket}:{prefix}";
            var prefixLength = $"{_keyPrefix}:{source}:{bucket}:".Length;

            return _s3.Keys
                .Where(x => x.StartsWith(prefixSearch))
                .Select(x => x.Substring(prefixLength))
                .ToArray()
                .LogValue(x => "Listing keys for bucket [{Bucket}] result: {HttpStatusCode}, using prefix: \"{Prefix}\". Keys found: {KeyCount}, is key count truncated: {IsTruncated}, iteration: {PassCount}.".WithArgs(bucket, HttpStatusCode.OK, prefix, x.Length, false, 1))
                .ToResponseAsync();
        }

        public Task<Response<byte[]>> DownloadFile(string source, string bucket, string key)
        {
            if (_s3.TryGetValue($"{_keyPrefix}:{source}:{bucket}:{key}", out var data)) {
                Log.Info("S3 file transfer progress for [{Key}]: {PercentDone}%.".WithArgs(key, 100));
                return Task.FromResult(Response.Create(data)).LogValue("Download status from S3 for [{Key}]: {HttpStatusCode}.".WithArgs(key, HttpStatusCode.OK));
            }

            return Task.FromResult(Response.Create(Array.Empty<byte>())).LogValue("The key [{Key}], does not exist in the bucket [{Bucket}].".WithArgs(key, bucket));
        }
    }
}

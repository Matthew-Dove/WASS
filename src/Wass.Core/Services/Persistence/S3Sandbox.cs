using Amazon.S3;
using ContainerExpressions.Containers;
using System.Collections.Concurrent;
using System.Net;

namespace Wass.Core.Services.Persistence
{
    public sealed class S3Sandbox : IS3
    {
        private static readonly ConcurrentDictionary<string, string> _s3 = new();

        private static readonly Task<Response<Unit>> _unitSuccess = Task.FromResult(Unit.ResponseSuccess);
        private static readonly Task<Response<Unit>> _unitError = Task.FromResult(Unit.ResponseError);
        private static readonly Task<Response<bool>> _true = Task.FromResult(Response.Create(true));
        private static readonly Task<Response<bool>> _false = Task.FromResult(Response.Create(false));

        public ValueTask<Response<bool>> DoesBucketExist(string bucket)
        {
            if (_s3.ContainsKey($"{nameof(bucket)}:{bucket}")) return ValueTask.FromResult(Response.Create(true));
            return ValueTask.FromResult(Response.Create(false)).LogValue("Does AWS S3 bucket [{Bucket}] exist: {Exists}.".WithArgs(bucket, false));
        }

        public Task<Response<Unit>> CreateBucket(string bucket)
        {
            if (_s3.ContainsKey($"{nameof(bucket)}:{bucket}")) return _unitSuccess.LogValue("The S3 bucket [{Bucket}] already exists.".WithArgs(bucket));
            _s3.TryAdd($"{nameof(bucket)}:{bucket}", default);
            return _unitSuccess.LogValue("Creating new bucket in S3 [{Bucket}], status: {HttpStatusCode}.".WithArgs(bucket, HttpStatusCode.OK));
        }

        public Task<Response<bool>> DoesFileExist(string bucket, string key)
        {
            if (_s3.ContainsKey($"{nameof(key)}:{key}")) return _true.LogValue("The file [{Key}] was found in the bucket [{Bucket}].".WithArgs(key, bucket));
            return _false.LogValue("The key [{Key}], does not exist in the bucket [{Bucket}].".WithArgs(key, bucket));
        }

        public Task<Response<Unit>> CreateFile(string bucket, string key, byte[] data, S3StorageClass storageClass)
        {
            Log.Info("S3 file transfer progress for [{Key}]: {PercentDone}%.".WithArgs(key, 100));
            if (_s3.ContainsKey($"{nameof(key)}:{key}")) return _unitError.LogErrorValue("At least one of the pre-conditions you specified did not hold.");
            _s3.TryAdd($"{nameof(key)}:{key}", default);
            return _unitSuccess.LogValue("Upload status to S3 for [{Key}]: {HttpStatusCode}.".WithArgs(key, HttpStatusCode.OK));
        }
    }
}

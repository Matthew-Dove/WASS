using Amazon.S3;
using System.Runtime.InteropServices;
using Wass.Core.Services.Persistence;

namespace Tests.Wass.Core.Services.Persistence
{
    public class S3Tests : TestBase
    {
        // Credit: Billy Jo Catbagan (https://unsplash.com/photos/PbS9rXhsYIU).
        private readonly string _key = @"Data/billy-jo-catbagan-PbS9rXhsYIU-unsplash.jpg";

        [Fact(Skip = "Run this to add your AWS Keys as environment variables.")]
        public void SetEnvironmentVariables()
        {
            string
                accessKeyId = "",       // Add your Access Key here.
                secretAccessKey = "",   // Add your Secret Key here.
                bucket = "";            // Add your Bucket Name here.

            var target = EnvironmentVariableTarget.User; // Or EnvironmentVariableTarget.Machine.
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                target = EnvironmentVariableTarget.Process;
            }

            Environment.SetEnvironmentVariable($"Destination__Sources__{Source}__AccessKeyId", accessKeyId, target);
            Environment.SetEnvironmentVariable($"Destination__Sources__{Source}__SecretAccessKey", secretAccessKey, target);
            Environment.SetEnvironmentVariable($"Destination__Sources__{Source}__Bucket", bucket, target);
        }

        [Fact(Skip = "Run this to upload the test file to a new bucket.")]
        public async Task CreateBucket_CreateFile()
        {
            var config = DestinationConfig;
            var bucketName = config.Value.Sources[Source].Bucket;
            var s3 = new S3(config);
            var data = File.ReadAllBytes($"./{_key}");

            var bucketExists = await s3.DoesBucketExist(Source, bucketName);
            Assert.True(bucketExists.IsValid);

            if (!bucketExists.Value)
            {
                var bucket = await s3.CreateBucket(Source, bucketName);
                Assert.True(bucket);
            }

            var fileExists = await s3.DoesFileExist(Source, bucketName, _key);
            Assert.True(fileExists.IsValid);

            if (!fileExists.Value)
            {
                var file = await s3.CreateFile(Source, bucketName, _key, data, S3StorageClass.IntelligentTiering);
                Assert.True(file);
            }
        }
    }
}

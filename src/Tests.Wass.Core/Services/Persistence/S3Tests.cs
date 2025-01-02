using Amazon.S3;
using System.Runtime.InteropServices;
using Wass.Core.Services.Persistence;

namespace Tests.Wass.Core.Services.Persistence
{
    public class S3Tests : TestBase
    {
        // TODO: Remove hardcoded bucket name.
        private readonly string _bucket = "wwbuqoiokwt2";// string.Empty.GenerateBucketName();

        // Credit: Billy Jo Catbagan (https://unsplash.com/photos/PbS9rXhsYIU).
        private readonly string _key = @"Data/billy-jo-catbagan-PbS9rXhsYIU-unsplash.jpg";

        [Fact(Skip = "Run this to add your AWS Keys as environment variables.")]
        public void SetEnvironmentVariable()
        {
            string
                accessKeyId = "",       // Add your Access Key here.
                secretAccessKey = "";   // Add your Secret Key here.

            var target = EnvironmentVariableTarget.User; // Or EnvironmentVariableTarget.Machine.
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                target = EnvironmentVariableTarget.Process;
            }

            // Replace "__S3__" with whatever your "Source" is called in the config file under "Destination".
            Environment.SetEnvironmentVariable("Destination__Sources__S3__AccessKeyId", accessKeyId, target);
            Environment.SetEnvironmentVariable("Destination__Sources__S3__SecretAccessKey", secretAccessKey, target);
        }

        [Fact(Skip = "Run this to upload the test file to a new bucket.")]
        public async Task CreateBucket_CreateFile()
        {
            var s3 = new S3(DestinationConfig);
            var data = File.ReadAllBytes($"./{_key}");

            var bucketExists = await s3.DoesBucketExist(Source, _bucket);
            Assert.True(bucketExists.IsValid);

            if (!bucketExists.Value)
            {
                var bucket = await s3.CreateBucket(Source, _bucket);
                Assert.True(bucket);
            }

            var fileExists = await s3.DoesFileExist(Source, _bucket, _key);
            Assert.True(fileExists.IsValid);

            if (!fileExists.Value)
            {
                var file = await s3.CreateFile(Source, _bucket, _key, data, S3StorageClass.IntelligentTiering);
                Assert.True(file);
            }
        }
    }
}

using Amazon.S3;
using Wass.Core.Models.Configuration;
using Wass.Core.Services.Persistence;

namespace Tests.Wass.Core.Services.Persistence
{
    public class S3Tests : TestBase
    {
        // TODO: Remove hardcoded bucket name.
        private readonly string _bucket = "wwbuqoiokwt2";// string.Empty.GenerateBucketName();

        // Credit: Billy Jo Catbagan (https://unsplash.com/photos/PbS9rXhsYIU).
        private readonly string _key = @"Data/billy-jo-catbagan-PbS9rXhsYIU-unsplash.jpg";

        [Fact(Skip = "Run this to add your AWS Keys as user environment variables (Windows only).")]
        public void SetEnvironmentVariable()
        {
            string
                accessKeyId = "",       // Add your Access Key here.
                secretAccessKey = "";   // Add your Secret Key here.

            Environment.SetEnvironmentVariable(S3Config.EnvironmentVariableAccessKeyId, accessKeyId, EnvironmentVariableTarget.User);
            Environment.SetEnvironmentVariable(S3Config.EnvironmentVariableSecretAccessKey, secretAccessKey, EnvironmentVariableTarget.User);
        }

        [Fact(Skip = "Run this to upload the test file to a new bucket.")]
        public async Task CreateBucket_CreateFile()
        {
            var s3 = new S3(S3Options);
            var data = File.ReadAllBytes($"./{_key}");

            var bucketExists = await s3.DoesBucketExist(_bucket);
            Assert.True(bucketExists.IsValid);

            if (!bucketExists.Value)
            {
                var bucket = await s3.CreateBucket(_bucket);
                Assert.True(bucket);
            }

            var fileExists = await s3.DoesFileExist(_bucket, _key);
            Assert.True(fileExists.IsValid);

            if (!fileExists.Value)
            {
                var file = await s3.CreateFile(_bucket, _key, data, S3StorageClass.IntelligentTiering);
                Assert.True(file);
            }
        }
    }
}

using Amazon.S3;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading.Tasks;
using Wass.Code.Persistence.Aws;

namespace Tests.Wass.Code.Persistence.Aws
{
    [TestClass]
    public class S3Tests
    {
        private readonly string _bucket = Path.GetRandomFileName().Replace(".", "").ToLower();

        // Credit: Billy Jo Catbagan (https://unsplash.com/photos/PbS9rXhsYIU).
        private readonly string _key = @"Data/billy-jo-catbagan-PbS9rXhsYIU-unsplash.jpg";

        // Run this test to add your AWS Keys as user environment variables (Windows only).
        [Ignore]
        [TestMethod]
        public void SetEnvironmentVariable()
        {
            string
                accessKeyId = "",
                secretAccessKey = "";

            Environment.SetEnvironmentVariable("WASS_AWS_S3_AccessKeyId", accessKeyId, EnvironmentVariableTarget.User);
            Environment.SetEnvironmentVariable("WASS_AWS_S3_SecretAccessKey", secretAccessKey, EnvironmentVariableTarget.User);
        }

        [Ignore]
        [TestMethod]
        public async Task Test()
        {
            await S3.CreateBucket(_bucket);
            var data = File.ReadAllBytes($"./{_key}");
            var result = await S3.Upload(_bucket, _key, data, S3StorageClass.Standard);
            Assert.IsTrue(result);
        }
    }
}

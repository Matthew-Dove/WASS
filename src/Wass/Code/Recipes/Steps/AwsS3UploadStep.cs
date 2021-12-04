﻿using Amazon.S3;
using Wass.Code.Infrastructure;
using Wass.Code.Persistence.Aws;
using Wass.Code.Persistence.Configuration;

namespace Wass.Code.Recipes.Steps
{
    public sealed class AwsS3UploadStep : Step
    {
        internal override string Version => "1.0.0";
        internal AwsS3UploadStep() : base(isAsync: true) { }
        internal override bool Method(FileModel file, IngredientModel ingredients) => throw new NotImplementedException();
        internal override Task<bool> MethodAsync(FileModel file, IngredientModel ingredients) => AwsS3Upload(file, ingredients);

        private static readonly string[] _storageClasses = new string[] {
            "DEEP_ARCHIVE", "GLACIER", "INTELLIGENT_TIERING", "STANDARD", "STANDARD_IA", "ONEZONE_IA"
        };

        private static async Task<bool> AwsS3Upload(FileModel file, IngredientModel ingredients)
        {
            if (!file.IsValid() || !ingredients.IsValid() || !Config.S3.IsValid()) return false.Trail($"{nameof(AwsS3UploadStep)} validation failed.");
            var isValid = false;
            string
                bucket = (ingredients["bucket"] ?? Config.S3.Bucket).ToLowerInvariant(),
                storage = (ingredients["storage"] ?? "INTELLIGENT_TIERING").ToUpperInvariant();

            try
            {
                var path = file.Path.GetNormalisedPath().Trail(x => $"Normalising file path from [{file.Path}], to [{x}] for S3 upload.");
                if (storage.IsEqualTo(_storageClasses) && bucket.IsBucketValid() && !string.IsNullOrEmpty(path))
                {
                    if (await S3.CreateBucket(bucket))
                    {
                        isValid = await S3.Upload(bucket, path, file.Data, S3StorageClass.FindValue(storage));
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(AwsS3UploadStep));
            }

            return isValid.Trail(x => $"Is {nameof(AwsS3UploadStep)} Valid: {x}.");
        }
    }
}
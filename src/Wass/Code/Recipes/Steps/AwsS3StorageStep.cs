using Amazon.S3;
using Wass.Code.Infrastructure;
using Wass.Code.Persistence.Aws;
using Wass.Code.Persistence.Configuration;

namespace Wass.Code.Recipes.Steps
{
    public sealed class AwsS3StorageStep : StorageStep
    {
        internal override string Version => "1.0.0";
        internal AwsS3StorageStep() : base(isAsync: true) { }
        internal override bool Method(FileModel file, IngredientModel ingredients) => throw new NotImplementedException();
        internal override Task<bool> MethodAsync(FileModel file, IngredientModel ingredients) => AwsS3Upload(file, ingredients);
        internal override Task<bool> DoesFileExist(string filepath, IngredientModel ingredients) => FileExists(filepath, ingredients);

        private static readonly string[] _storageClasses = new string[] {
            "DEEP_ARCHIVE", "GLACIER", "INTELLIGENT_TIERING", "STANDARD", "STANDARD_IA", "ONEZONE_IA"
        };

        private static async Task<bool> AwsS3Upload(FileModel file, IngredientModel ingredients)
        {
            if (!file.IsValid() || !ingredients.IsValid() || !Config.S3.IsValid()) return false.Trail($"{nameof(AwsS3StorageStep)} validation failed.");
            var isValid = false;
            string
                bucket = (ingredients["bucket"] ?? Config.S3.Bucket).ToLowerInvariant(),
                storage = (ingredients["storage"] ?? "INTELLIGENT_TIERING").ToUpperInvariant();

            try
            {
                var path = file.Path.GetNormalisedPath().Trail(x => $"Normalising file path from [{file.Path}], to [{x}] for S3 upload.");
                if (storage.IsEqualTo(_storageClasses) && bucket.IsBucketValid() && !string.IsNullOrEmpty(path))
                {
                    if (await S3.DoesBucketExist(bucket) || await S3.CreateBucket(bucket))
                    {
                        isValid = await S3.Upload(bucket, path, file.Data, S3StorageClass.FindValue(storage));
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(AwsS3StorageStep));
            }

            return isValid.Trail(x => $"Is {nameof(AwsS3StorageStep)} Valid: {x}.");
        }

        private static async Task<bool> FileExists(string filepath, IngredientModel ingredients)
        {
            filepath.Guard(nameof(filepath));
            if (!ingredients.IsValid() || !Config.S3.IsValid()) return false.Trail($"{nameof(FileExists)} validation failed.");
            var isValid = false;
            string bucket = (ingredients["bucket"] ?? Config.S3.Bucket).ToLowerInvariant();

            try
            {
                var path = filepath.GetNormalisedPath();
                if (bucket.IsBucketValid() && !string.IsNullOrEmpty(path))
                {
                    isValid = await S3.DoesBucketExist(bucket) && await S3.DoesFileExist(bucket, path);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(FileExists));
            }

            return isValid.Trail(x => $"Is {nameof(FileExists)} Valid: {x}.");
        }
    }
}
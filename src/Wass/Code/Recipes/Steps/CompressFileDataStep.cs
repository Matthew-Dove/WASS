using Wass.Code.Compression;
using Wass.Code.Infrastructure;

namespace Wass.Code.Recipes.Steps
{
    public sealed class CompressFileDataStep : Step
    {
        internal override string Version => "1.0.0";
        internal CompressFileDataStep() : base(isAsync: false) { }
        internal override bool Method(FileModel file, IngredientModel ingredients) => CompressFileData(file);
        internal override Task<bool> MethodAsync(FileModel file, IngredientModel ingredients) => throw new NotImplementedException();

        private static bool CompressFileData(FileModel file)
        {
            if (!file.IsValid()) return false.Trail($"{nameof(CompressFileDataStep)} validation failed.");
            var isValid = false;

            try
            {
                var compressedBytes = GZip.Compress(file.Data);
                file = file.WithData(compressedBytes);
                isValid = true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(CompressFileDataStep));
            }

            return isValid.Trail(x => $"Is {nameof(CompressFileDataStep)} Valid: {x}.");
        }
    }
}

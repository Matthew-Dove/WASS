using Wass.Code.Compression;
using Wass.Code.Infrastructure;

namespace Wass.Code.Recipes.Steps
{
    public sealed class CompressFileDataStep : Step
    {
        internal CompressFileDataStep() : base(isAsync: false) { }
        internal override bool Method(ref FileModel file, IngredientModel ingredients) => CompressFileData(ref file);
        internal override Task<bool> MethodAsync(ref FileModel file, IngredientModel ingredients) => throw new NotImplementedException();

        private static bool CompressFileData(ref FileModel file)
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

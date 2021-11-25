using System.Text;
using Wass.Code.Compression;
using Wass.Code.Infrastructure;

namespace Wass.Code.Recipes.Steps
{
    public sealed class CompressFilePathStep : Step
    {
        internal CompressFilePathStep() : base(isAsync: false) { }
        internal override bool Method(ref FileModel file, IngredientModel ingredients) => CompressFilePath(ref file);
        internal override Task<bool> MethodAsync(ref FileModel file, IngredientModel ingredients) => throw new NotImplementedException();

        private static bool CompressFilePath(ref FileModel file)
        {
            if (!file.IsValid()) return false.Trail($"{nameof(CompressFilePathStep)} validation failed.");
            var isValid = false;
            byte[] compressedBytes = null;

            try
            {
                compressedBytes = GZip.Compress(Encoding.UTF8.GetBytes(file.Location));
                file = file.WithLocation(Encoding.UTF8.GetString(compressedBytes));

                compressedBytes = GZip.Compress(Encoding.UTF8.GetBytes(file.Name));
                file = file.WithName(Encoding.UTF8.GetString(compressedBytes));

                compressedBytes = GZip.Compress(Encoding.UTF8.GetBytes(file.Extension));
                file = file.WithExtension(Encoding.UTF8.GetString(compressedBytes));

                isValid = true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(CompressFilePathStep));
            }

            return isValid.Trail(x => $"Is {nameof(CompressFilePathStep)} Valid: {x}.");
        }
    }
}

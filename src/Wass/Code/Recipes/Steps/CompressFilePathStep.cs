using System.Text;
using Wass.Code.Compression;
using Wass.Code.Infrastructure;

namespace Wass.Code.Recipes.Steps
{
    public sealed class CompressFilePathStep : Step
    {
        internal CompressFilePathStep() : base(isAsync: false) { }
        internal override bool Method(FileModel file, IngredientModel ingredients) => CompressFilePath(file);
        internal override Task<bool> MethodAsync(FileModel file, IngredientModel ingredients) => throw new NotImplementedException();

        private static bool CompressFilePath(FileModel file)
        {
            if (!file.IsValid()) return false.Trail($"{nameof(CompressFilePathStep)} validation failed.");
            var isValid = false;

            try
            {
                var pathBytes = GZip.Compress(Encoding.UTF8.GetBytes(file.Path));
                var path = Encoding.UTF8.GetString(pathBytes);
                file = file.WithPath(path);
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

using System.Text;
using Wass.Code.Compression;
using Wass.Code.Infrastructure;

namespace Wass.Code.Recipes.Steps
{
    public sealed class CompressFilePathStep : Step
    {
        public CompressFilePathStep() : base(isAsync: false) { }
        internal override Task<bool> MethodAsync(ref IngredientModel ingredients) => throw new NotImplementedException();
        internal override bool Method(ref IngredientModel ingredients) => CompressFilePath(ref ingredients);

        private static bool CompressFilePath(ref IngredientModel ingredients)
        {
            if (!ingredients.IsValid()) return false.Trail($"{nameof(CompressFilePathStep)} validation failed.");
            var isValid = false;
            byte[] compressedBytes = null;

            try
            {
                compressedBytes = GZip.Compress(Encoding.UTF8.GetBytes(ingredients.Path));
                ingredients = ingredients.WithPath(Encoding.UTF8.GetString(compressedBytes));

                compressedBytes = GZip.Compress(Encoding.UTF8.GetBytes(ingredients.Name));
                ingredients = ingredients.WithName(Encoding.UTF8.GetString(compressedBytes));

                compressedBytes = GZip.Compress(Encoding.UTF8.GetBytes(ingredients.Extension));
                ingredients = ingredients.WithExtension(Encoding.UTF8.GetString(compressedBytes));

                isValid = true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(CompressFilePathStep));
            }

            return isValid;
        }
    }
}

using Wass.Code.Compression;
using Wass.Code.Infrastructure;

namespace Wass.Code.Recipes.Steps
{
    public sealed class CompressFileDataStep : Step
    {
        public CompressFileDataStep() : base(isAsync: false) { }
        internal override bool Method(ref IngredientModel ingredients) => CompressFileData(ref ingredients);
        internal override Task<bool> MethodAsync(ref IngredientModel ingredients) => throw new NotImplementedException();

        private static bool CompressFileData(ref IngredientModel ingredients)
        {
            if (!ingredients.IsValid()) return false.Trail($"{nameof(CompressFileDataStep)} validation failed.");
            var isValid = false;

            try
            {
                var compressedBytes = GZip.Compress(ingredients.Data);
                ingredients = ingredients.WithData(compressedBytes);
                isValid = true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(CompressFileDataStep));
            }

            return isValid;
        }
    }
}

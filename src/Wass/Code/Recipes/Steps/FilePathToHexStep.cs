using System.Text;
using Wass.Code.Infrastructure;

namespace Wass.Code.Recipes.Steps
{
    public sealed class FilePathToHexStep : Step
    {
        public FilePathToHexStep() : base(isAsync: false) { }
        internal override Task<bool> MethodAsync(ref IngredientModel ingredients) => throw new NotImplementedException();
        internal override bool Method(ref IngredientModel ingredients) => FilePathToHex(ref ingredients);

        private static bool FilePathToHex(ref IngredientModel ingredients)
        {
            if (!ingredients.IsValid()) return false.Trail($"{nameof(FilePathToHexStep)} validation failed.");
            var isValid = false;
            string hex = null;

            try
            {
                hex = Convert.ToHexString(Encoding.UTF8.GetBytes(ingredients.Path));
                ingredients = ingredients.WithPath(hex);

                hex = Convert.ToHexString(Encoding.UTF8.GetBytes(ingredients.Name));
                ingredients = ingredients.WithName(hex);

                hex = Convert.ToHexString(Encoding.UTF8.GetBytes(ingredients.Extension));
                ingredients = ingredients.WithExtension(hex);

                isValid = true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(FilePathToHexStep));
            }

            return isValid;
        }
    }
}

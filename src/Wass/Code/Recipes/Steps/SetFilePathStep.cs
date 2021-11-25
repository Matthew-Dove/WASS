using Wass.Code.Infrastructure;

namespace Wass.Code.Recipes.Steps
{
    public sealed class SetFilePathStep : Step
    {
        public SetFilePathStep() : base(isAsync: false) { }
        internal override bool Method(ref IngredientModel ingredients) => SetFilePath(ref ingredients);
        internal override Task<bool> MethodAsync(ref IngredientModel ingredients) => throw new NotImplementedException();

        private static bool SetFilePath(ref IngredientModel ingredients)
        {
            if (!ingredients.IsValid()) return false.Trail($"{nameof(SetFilePathStep)} validation failed.");
            var isValid = false;
            string path = ingredients.Path, name = ingredients.Name, extension = ingredients.Extension;

            try
            {
                isValid = true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(SetFilePathStep));
            }

            return isValid;
        }
    }
}

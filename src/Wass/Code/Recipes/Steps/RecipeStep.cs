using Wass.Code.Infrastructure;

namespace Wass.Code.Recipes.Steps
{
    public sealed class RecipeStep : Step
    {
        internal RecipeStep() : base(isAsync: false) { }
        internal override bool Method(ref FileModel file, IngredientModel ingredients) => Recipe(ref file, ingredients);
        internal override Task<bool> MethodAsync(ref FileModel file, IngredientModel ingredients) => throw new NotImplementedException();

        private static readonly string[] _requiredIngredients = { "names" };

        private static bool Recipe(ref FileModel file, IngredientModel ingredients)
        {
            if (!file.IsValid() || !ingredients.IsValid(_requiredIngredients)) return false.Trail($"{nameof(RecipeStep)} validation failed.");
            var isValid = false;

            try
            {
                isValid = true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(RecipeStep));
            }

            return isValid.Trail(x => $"Is {nameof(RecipeStep)} Valid: {x}.");
        }
    }
}

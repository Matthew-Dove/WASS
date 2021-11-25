using Wass.Code.Infrastructure;

namespace Wass.Code.Recipes.Steps
{
    public sealed class TimeDelayStep : Step
    {
        internal TimeDelayStep() : base(isAsync: false) { }
        internal override bool Method(FileModel file, IngredientModel ingredients) => TimeDelay(file, ingredients);
        internal override Task<bool> MethodAsync(FileModel file, IngredientModel ingredients) => throw new NotImplementedException();

        private static readonly string[] _requiredIngredients = { "ms" };

        private static bool TimeDelay(FileModel file, IngredientModel ingredients)
        {
            if (!file.IsValid() || !ingredients.IsValid(_requiredIngredients)) return false.Trail($"{nameof(TimeDelayStep)} validation failed.");
            var isValid = false;
            var ms = ingredients["ms"];

            try
            {
                if (int.TryParse(ms, out int milliseconds) && milliseconds > 0)
                {
                    Thread.Sleep(milliseconds);
                    isValid = true;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(TimeDelayStep));
            }

            return isValid.Trail(x => $"Is {nameof(TimeDelayStep)} Valid: {x}.");
        }
    }
}

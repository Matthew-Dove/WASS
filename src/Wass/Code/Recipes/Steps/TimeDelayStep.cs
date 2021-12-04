using Wass.Code.Infrastructure;

namespace Wass.Code.Recipes.Steps
{
    public sealed class TimeDelayStep : Step
    {
        internal override string Version => "1.0.0";
        internal TimeDelayStep() : base(isAsync: true) { }
        internal override bool Method(FileModel file, IngredientModel ingredients) => throw new NotImplementedException();
        internal override Task<bool> MethodAsync(FileModel file, IngredientModel ingredients) => TimeDelay(file, ingredients);

        private static readonly string[] _requiredIngredients = { "ms" };

        private static async Task<bool> TimeDelay(FileModel file, IngredientModel ingredients)
        {
            if (!file.IsValid() || !ingredients.IsValid(_requiredIngredients)) return false.Trail($"{nameof(TimeDelayStep)} validation failed.");
            var isValid = false;
            var ms = ingredients["ms"];

            try
            {
                if (int.TryParse(ms, out int milliseconds) && milliseconds > 0)
                {
                    await Task.Delay(milliseconds);
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

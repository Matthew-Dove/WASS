namespace Wass.Code.Recipes
{
    public sealed class InstructionModel
    {
        public Step Step { get; }
        public IngredientModel Ingredients { get; }
        public bool ContinueOnError { get; }

        private static readonly IngredientModel _ingredientsCache = new();

        public InstructionModel(Step step, bool continueOnError = false)
        {
            if (step == null) throw new ArgumentNullException(nameof(step));

            Step = step;
            Ingredients = _ingredientsCache;
            ContinueOnError = continueOnError;
        }

        public InstructionModel(Step step, IngredientModel ingredients, bool continueOnError = false)
        {
            if (step == null) throw new ArgumentNullException(nameof(step));
            if (ingredients == null || ingredients.Count < 1) throw new ArgumentException("Length must be greater than 0.", nameof(ingredients));

            Step = step;
            Ingredients = ingredients;
            ContinueOnError = continueOnError;
        }
    }
}

using Wass.Code.Infrastructure;

namespace Wass.Code.Recipes.Steps
{
    public sealed class FilterFileSizeStep : Step
    {
        internal override string Version => "1.0.0";
        internal FilterFileSizeStep() : base(isAsync: false) { }
        internal override bool Method(FileModel file, IngredientModel ingredients) => FilterFileSize(file, ingredients);
        internal override Task<bool> MethodAsync(FileModel file, IngredientModel ingredients) => throw new NotImplementedException();

        private const long B = 1;
        private const long KB = 1024;
        private const long MB = 1024 * 1024;
        private const long GB = 1024 * 1024 * 1024;

        private static readonly string[] _requiredIngredients = { "comparison", "size", "unit" };

        private static bool FilterFileSize(FileModel file, IngredientModel ingredients)
        {
            if (!file.IsValid() || !ingredients.IsValid(_requiredIngredients)) return false.Trail($"{nameof(FilterFileSizeStep)} validation failed.");
            var isValid = false;
            string comparison = ingredients["comparison"], size = ingredients["size"], unit = ingredients["unit"];

            try
            {
                var comparisonOperator = string.Empty;
                if (comparison.IsEqualTo("GreaterThan")) comparisonOperator = ">";
                if (comparison.IsEqualTo("GreaterThanOrEqualTo")) comparisonOperator = ">=";
                if (comparison.IsEqualTo("LessThan")) comparisonOperator = "<";
                if (comparison.IsEqualTo("LessThanOrEqualTo")) comparisonOperator = "<=";

                var unitMultiplier = 0L;
                if (unit.IsEqualTo("B")) unitMultiplier = B;
                if (unit.IsEqualTo("KB")) unitMultiplier = KB;
                if (unit.IsEqualTo("MB")) unitMultiplier = MB;
                if (unit.IsEqualTo("GB")) unitMultiplier = GB;

                long.TryParse(size, out long targetSize);
                var filesize = file.Data.LongLength * unitMultiplier;

                if (filesize > 0L && targetSize > 0L && unitMultiplier != 0L && comparisonOperator != string.Empty)
                {
                    var keepFile = (comparisonOperator switch
                    {
                        ">" => filesize > targetSize,
                        ">=" => filesize >= targetSize,
                        "<" => filesize < targetSize,
                        "<=" => filesize <= targetSize,
                        _ => false
                    }).Trail(x => $"Did {nameof(FilterFileSizeStep)} decide to keep the file: {x}.");
                    if (!keepFile) file = file.WithData(Array.Empty<byte>());
                    isValid = true;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(FilterFileSizeStep));
            }

            return isValid.Trail(x => $"Is {nameof(FilterFileSizeStep)} Valid: {x}.");
        }
    }
}

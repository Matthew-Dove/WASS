using System.Collections.Specialized;
using Wass.Code.Infrastructure;

namespace Wass.Code.Recipes
{
    public sealed class IngredientModel : StringDictionary { }

    internal static class IngredientModelExtensions
    {
        public static bool IsValid(this IngredientModel ingredients, params string[] keys)
        {
            if (ingredients == null) return false.Trail($"{nameof(IngredientModel)} is null, validation failed.");

            foreach (var key in keys)
            {
                var value = ingredients[key];
                if (string.IsNullOrEmpty(value)) return false.Trail($"The key {key}, has no matching value from {nameof(IngredientModel)}.");
            }

            return true;
        }
    }
}

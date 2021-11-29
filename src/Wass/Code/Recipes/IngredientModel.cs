using System.Collections.Specialized;
using Wass.Code.Infrastructure;

namespace Wass.Code.Recipes
{
    public sealed class IngredientModel
    {
        private readonly StringDictionary _ingredients = new();

        public string this[string key] { get { return _ingredients[key]; } set { _ingredients[key] = value; } }

        public void Add(string key, string value) => _ingredients.Add(key, value);

        public int Count => _ingredients.Count;
    }

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

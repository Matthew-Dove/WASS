namespace Wass.Code.Recipes
{
    internal readonly struct IngredientModel
    {
        /// <summary>The file's data (raw bytes may have been transformed e.g. compressed / encrypted.).</summary>
        public byte[] Data { get; }

        /// <summary>The file's path: "C:\personal\docs".</summary>
        public string Path { get; }

        /// <summary>The file's name: "birthdays".</summary>
        public string Name { get; }

        /// <summary>The file's extension: "txt".</summary>
        public string Extension { get; }

        public IngredientModel(byte[] data, string path, string name, string extension)
        {
            Data = data;
            Path = path;
            Name = name;
            Extension = extension;
        }
    }

    internal static class IngredientModelExtensions
    {
        public static IngredientModel WithData(this IngredientModel ingredients, byte[] data) => new (data, ingredients.Path, ingredients.Name, ingredients.Extension);
        public static IngredientModel WithPath(this IngredientModel ingredients, string path) => new (ingredients.Data, path, ingredients.Name, ingredients.Extension);
        public static IngredientModel WithName(this IngredientModel ingredients, string name) => new (ingredients.Data, ingredients.Path, name, ingredients.Extension);
        public static IngredientModel WithExtension(this IngredientModel ingredients, string extension) => new (ingredients.Data, ingredients.Path, ingredients.Name, extension);

        public static bool IsValid(this IngredientModel ingredients)
        {
            return
                ingredients.Data != null &&
                ingredients.Data.Length > 0 &&
                !string.IsNullOrEmpty(ingredients.Path) &&
                !string.IsNullOrEmpty(ingredients.Name) &&
                !string.IsNullOrEmpty(ingredients.Extension);
        }
    }
}

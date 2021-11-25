using Wass.Code.Infrastructure;

namespace Wass.Code.Recipes.Steps
{
    public sealed class SetFilePathStep : Step
    {
        internal SetFilePathStep() : base(isAsync: false) { }
        internal override bool Method(FileModel file, IngredientModel ingredients) => SetFilePath(file, ingredients);
        internal override Task<bool> MethodAsync(FileModel file, IngredientModel ingredients) => throw new NotImplementedException();

        private static readonly string[] _requiredIngredients = { "path" };

        private static bool SetFilePath(FileModel file, IngredientModel ingredients)
        {
            if (!file.IsValid() || !ingredients.IsValid(_requiredIngredients)) return false.Trail($"{nameof(SetFilePathStep)} validation failed.");
            var isValid = false;
            var path = ingredients["path"];

            try
            {
                if (path.Contains("{{") && path.Contains("}}"))
                {
                    var currentPath = file.GetPath();
                    path = path
                        .Replace("{{location}}", file.Location)
                        .Replace("{{name}}", file.Name)
                        .Replace("{{extension}}", file.Extension)
                        .Trail(newPath => $"Changing the file's path from [{currentPath}], to [{newPath}] in {nameof(SetFilePathStep)}.");
                }

                if (path.TrySplitPath(out (string Location, string Name, string Extension) splitPath))
                {
                    file = file
                        .WithLocation(splitPath.Location)
                        .WithName(splitPath.Name)
                        .WithExtension(splitPath.Extension);

                    isValid = true;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(SetFilePathStep));
            }

            return isValid.Trail(x => $"Is {nameof(SetFilePathStep)} Valid: {x}.");
        }
    }
}

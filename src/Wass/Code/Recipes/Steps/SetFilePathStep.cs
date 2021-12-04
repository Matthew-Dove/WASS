using Wass.Code.Infrastructure;

namespace Wass.Code.Recipes.Steps
{
    public sealed class SetFilePathStep : Step
    {
        internal override string Version => "1.0.0";
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
                var currentPath = file.Path;
                if (path.Contains("{{") && path.Contains("}}") && file.Path.TrySplitPath(out (string Directory, string Name, string Extension) splitPath))
                {
                    path = path
                        .Replace("{{directory}}", splitPath.Directory)
                        .Replace("{{name}}", splitPath.Name)
                        .Replace("{{extension}}", splitPath.Extension)
                        .Trail(x => $"Changing the file's path from [{file.Path}], to [{x}] in {nameof(SetFilePathStep)}.");
                }

                if (path.TrySplitPath(out (string Directory, string Name, string Extension) sp))
                {
                    path = Path.Combine(sp.Directory, sp.Name) + sp.Extension;
                    file = file.WithPath(path);
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

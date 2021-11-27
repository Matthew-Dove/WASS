using System.Text.RegularExpressions;
using Wass.Code.Infrastructure;

namespace Wass.Code.Recipes.Steps
{
    public sealed class FilterFilePathStep : Step
    {
        internal FilterFilePathStep() : base(isAsync: false) { }
        internal override bool Method(FileModel file, IngredientModel ingredients) => FilterFilePath(file, ingredients);
        internal override Task<bool> MethodAsync(FileModel file, IngredientModel ingredients) => throw new NotImplementedException();

        private static readonly string[] _requiredIngredients = { "regex", "match", "search" };

        private static bool FilterFilePath(FileModel file, IngredientModel ingredients)
        {
            if (!file.IsValid() || !ingredients.IsValid(_requiredIngredients)) return false.Trail($"{nameof(FilterFilePathStep)} validation failed.");
            var isValid = false;
            string regex = ingredients["regex"], match = ingredients["match"], search = ingredients["search"];

            try
            {
                string
                    matchBehaviour = match.IsEqualTo("include", "exclude") ? match :string.Empty,
                    searchArea = search.IsEqualTo("path", "directory", "name", "extension") ? search : string.Empty;

                if (matchBehaviour != string.Empty && searchArea != string.Empty)
                {
                    var target = searchArea.ToUpperInvariant() switch
                    {
                        "PATH" => file.GetPath(),
                        "DIRECTORY" => file.Directory,
                        "NAME" => file.Name,
                        "EXTENSION" => file.Extension,
                        _ => string.Empty
                    };

                    if (target != string.Empty)
                    {
                        var regularExpression = new Regex(regex, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                        var matches = regularExpression.Matches(target);
                        var keepFile = (matchBehaviour.ToUpperInvariant() switch
                        {
                            "INCLUDE" => matches.Count > 0,
                            "EXCLUDE" => matches.Count == 0,
                            _ => false
                        }).Trail(x => $"Did {nameof(FilterFilePathStep)} decide to keep the file: {x}.");
                        if (!keepFile) file = file.WithData(Array.Empty<byte>());
                    }

                    isValid = true;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(FilterFilePathStep));
            }

            return isValid.Trail(x => $"Is {nameof(FilterFilePathStep)} Valid: {x}.");
        }
    }
}

using System.Text.RegularExpressions;
using Wass.Code.Infrastructure;

namespace Wass.Code.Recipes.Steps
{
    public sealed class FilterFilePathStep : Step
    {
        internal FilterFilePathStep() : base(isAsync: false) { }
        internal override bool Method(ref FileModel file, IngredientModel ingredients) => FilterFilePath(ref file, ingredients);
        internal override Task<bool> MethodAsync(ref FileModel file, IngredientModel ingredients) => throw new NotImplementedException();

        private static readonly string[] _requiredIngredients = { "regex", "match", "search" };

        private static bool FilterFilePath(ref FileModel file, IngredientModel ingredients)
        {
            if (!file.IsValid() || !ingredients.IsValid(_requiredIngredients)) return false.Trail($"{nameof(FilterFilePathStep)} validation failed.");
            var isValid = false;
            string regex = ingredients["regex"], match = ingredients["match"], search = ingredients["search"];

            try
            {
                var regularExpression = new Regex(regex, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

                var matchBehaviour = string.Empty;
                if (match.IsEqualTo("include") || match.IsEqualTo("exclude")) matchBehaviour = match.ToUpperInvariant();

                var searchArea = string.Empty;
                if (search.IsEqualTo("path") || search.IsEqualTo("location") || search.IsEqualTo("name") || search.IsEqualTo("extension")) searchArea = search.ToUpperInvariant();

                if (matchBehaviour != string.Empty && searchArea != string.Empty)
                {
                    var target = searchArea switch
                    {
                        "PATH" => file.GetPath(),
                        "LOCATION" => file.Location,
                        "NAME" => file.Name,
                        "EXTENSION" => file.Extension,
                        _ => string.Empty
                    };

                    if (target != string.Empty)
                    {
                        var matches = regularExpression.Matches(target);
                        var keepFile = (matchBehaviour switch
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

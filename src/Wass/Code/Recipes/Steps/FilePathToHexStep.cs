using System.Text;
using Wass.Code.Infrastructure;

namespace Wass.Code.Recipes.Steps
{
    public sealed class FilePathToHexStep : Step
    {
        internal FilePathToHexStep() : base(isAsync: false) { }
        internal override bool Method(FileModel file, IngredientModel ingredients) => FilePathToHex(file);
        internal override Task<bool> MethodAsync(FileModel file, IngredientModel ingredients) => throw new NotImplementedException();

        private static bool FilePathToHex(FileModel file)
        {
            if (!file.IsValid()) return false.Trail($"{nameof(FilePathToHexStep)} validation failed.");
            var isValid = false;

            try
            {
                var path = Convert.ToHexString(Encoding.UTF8.GetBytes(file.Path));
                file = file.WithPath(path);
                isValid = true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(FilePathToHexStep));
            }

            return isValid.Trail(x => $"Is {nameof(FilePathToHexStep)} Valid: {x}.");
        }
    }
}

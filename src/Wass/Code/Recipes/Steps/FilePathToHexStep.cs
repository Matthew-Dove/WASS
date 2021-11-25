using System.Text;
using Wass.Code.Infrastructure;

namespace Wass.Code.Recipes.Steps
{
    public sealed class FilePathToHexStep : Step
    {
        internal FilePathToHexStep() : base(isAsync: false) { }
        internal override bool Method(ref FileModel file, IngredientModel ingredients) => FilePathToHex(ref file);
        internal override Task<bool> MethodAsync(ref FileModel file, IngredientModel ingredients) => throw new NotImplementedException();

        private static bool FilePathToHex(ref FileModel file)
        {
            if (!file.IsValid()) return false.Trail($"{nameof(FilePathToHexStep)} validation failed.");
            var isValid = false;
            string hex = null;

            try
            {
                hex = Convert.ToHexString(Encoding.UTF8.GetBytes(file.Location));
                file = file.WithLocation(hex);

                hex = Convert.ToHexString(Encoding.UTF8.GetBytes(file.Name));
                file = file.WithName(hex);

                hex = Convert.ToHexString(Encoding.UTF8.GetBytes(file.Extension));
                file = file.WithExtension(hex);

                isValid = true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(FilePathToHexStep));
            }

            return isValid;
        }
    }
}

using System.Collections.Specialized;
using System.Text;
using Wass.Code.Infrastructure;

namespace Wass.Code.Recipes.Steps
{
    public sealed class FilePathToHexStep : Step
    {
        public FilePathToHexStep() : base(isAsync: false) { }
        internal override bool Method(ref FileModel file, ListDictionary ingredients) => FilePathToHex(ref file, ingredients);
        internal override Task<bool> MethodAsync(ref FileModel file, ListDictionary ingredients) => throw new NotImplementedException();

        private static bool FilePathToHex(ref FileModel file, ListDictionary ingredients)
        {
            if (!file.IsValid()) return false.Trail($"{nameof(FilePathToHexStep)} validation failed.");
            var isValid = false;
            string hex = null;

            try
            {
                hex = Convert.ToHexString(Encoding.UTF8.GetBytes(file.Path));
                file = file.WithPath(hex);

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

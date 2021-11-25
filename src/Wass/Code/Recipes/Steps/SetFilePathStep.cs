using System.Collections.Specialized;
using Wass.Code.Infrastructure;

namespace Wass.Code.Recipes.Steps
{
    public sealed class SetFilePathStep : Step
    {
        public SetFilePathStep() : base(isAsync: false) { }
        internal override bool Method(ref FileModel file, ListDictionary ingredients) => SetFilePath(ref file, ingredients);
        internal override Task<bool> MethodAsync(ref FileModel file, ListDictionary ingredients) => throw new NotImplementedException();

        private static bool SetFilePath(ref FileModel file, ListDictionary ingredients)
        {
            if (!file.IsValid()) return false.Trail($"{nameof(SetFilePathStep)} validation failed.");
            var isValid = false;
            string path = file.Path, name = file.Name, extension = file.Extension;

            try
            {
                isValid = true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(SetFilePathStep));
            }

            return isValid;
        }
    }
}

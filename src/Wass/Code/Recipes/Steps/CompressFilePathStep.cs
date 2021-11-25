using System.Collections.Specialized;
using System.Text;
using Wass.Code.Compression;
using Wass.Code.Infrastructure;

namespace Wass.Code.Recipes.Steps
{
    public sealed class CompressFilePathStep : Step
    {
        public CompressFilePathStep() : base(isAsync: false) { }
        internal override bool Method(ref FileModel file, ListDictionary ingredients) => CompressFilePath(ref file, ingredients);
        internal override Task<bool> MethodAsync(ref FileModel file, ListDictionary ingredients) => throw new NotImplementedException();

        private static bool CompressFilePath(ref FileModel file, ListDictionary ingredients)
        {
            if (!file.IsValid()) return false.Trail($"{nameof(CompressFilePathStep)} validation failed.");
            var isValid = false;
            byte[] compressedBytes = null;

            try
            {
                compressedBytes = GZip.Compress(Encoding.UTF8.GetBytes(file.Path));
                file = file.WithPath(Encoding.UTF8.GetString(compressedBytes));

                compressedBytes = GZip.Compress(Encoding.UTF8.GetBytes(file.Name));
                file = file.WithName(Encoding.UTF8.GetString(compressedBytes));

                compressedBytes = GZip.Compress(Encoding.UTF8.GetBytes(file.Extension));
                file = file.WithExtension(Encoding.UTF8.GetString(compressedBytes));

                isValid = true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(CompressFilePathStep));
            }

            return isValid;
        }
    }
}

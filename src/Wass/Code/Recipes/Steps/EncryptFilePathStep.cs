using System.Text;
using Wass.Code.Encryption;
using Wass.Code.Infrastructure;
using Wass.Code.Persistence.Configuration;

namespace Wass.Code.Recipes.Steps
{
    public sealed class EncryptFilePathStep : Step
    {
        internal EncryptFilePathStep() : base(isAsync: false) { }
        internal override bool Method(FileModel file, IngredientModel ingredients) => EncryptFilePath(file);
        internal override Task<bool> MethodAsync(FileModel file, IngredientModel ingredients) => throw new NotImplementedException();

        private static bool EncryptFilePath(FileModel file)
        {
            if (!file.IsValid() || !Config.Encryption.IsValid()) return false.Trail($"{nameof(EncryptFilePathStep)} validation failed.");
            var isValid = false;

            try
            {
                var pathBytes = Aes.Encrypt(Config.Encryption.Password, Encoding.UTF8.GetBytes(file.Path));
                var path = Encoding.UTF8.GetString(pathBytes);
                file = file.WithPath(path);
                isValid = true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(EncryptFilePathStep));
            }

            return isValid.Trail(x => $"Is {nameof(EncryptFilePathStep)} Valid: {x}.");
        }
    }
}

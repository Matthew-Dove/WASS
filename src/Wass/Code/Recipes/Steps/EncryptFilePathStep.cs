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
                var directoryBytes = Aes.Encrypt(Config.Encryption.Password, Encoding.UTF8.GetBytes(file.Directory));
                var nameBytes = Aes.Encrypt(Config.Encryption.Password, Encoding.UTF8.GetBytes(file.Name));
                var extensionBytes = Aes.Encrypt(Config.Encryption.Password, Encoding.UTF8.GetBytes(file.Extension));

                var directory = Encoding.UTF8.GetString(directoryBytes);
                var name = Encoding.UTF8.GetString(nameBytes);
                var extension = Encoding.UTF8.GetString(extensionBytes);

                file = file
                    .WithDirectory(directory)
                    .WithName(name)
                    .WithExtension(extension);

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

using System.Text;
using Wass.Code.Encryption;
using Wass.Code.Infrastructure;
using Wass.Code.Persistence.Configuration;

namespace Wass.Code.Recipes.Steps
{
    public sealed class EncryptFilePathStep : Step
    {
        internal EncryptFilePathStep() : base(isAsync: false) { }
        internal override bool Method(ref FileModel file, IngredientModel ingredients) => EncryptFilePath(ref file);
        internal override Task<bool> MethodAsync(ref FileModel file, IngredientModel ingredients) => throw new NotImplementedException();

        private static bool EncryptFilePath(ref FileModel file)
        {
            if (!file.IsValid() || !Config.Encryption.IsValid()) return false.Trail($"{nameof(EncryptFilePathStep)} validation failed.");
            var isValid = false;
            byte[] cipherbytes = null;

            try
            {
                cipherbytes = Aes.Encrypt(Config.Encryption.Password, Encoding.UTF8.GetBytes(file.Location));
                file = file.WithLocation(Encoding.UTF8.GetString(cipherbytes));

                cipherbytes = Aes.Encrypt(Config.Encryption.Password, Encoding.UTF8.GetBytes(file.Name));
                file = file.WithName(Encoding.UTF8.GetString(cipherbytes));

                cipherbytes = Aes.Encrypt(Config.Encryption.Password, Encoding.UTF8.GetBytes(file.Extension));
                file = file.WithExtension(Encoding.UTF8.GetString(cipherbytes));

                isValid = true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(EncryptFilePathStep));
            }

            return isValid;
        }
    }
}

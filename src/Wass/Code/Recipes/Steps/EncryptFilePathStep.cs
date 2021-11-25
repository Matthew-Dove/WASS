using System.Text;
using Wass.Code.Encryption;
using Wass.Code.Infrastructure;
using Wass.Code.Persistence.Configuration;

namespace Wass.Code.Recipes.Steps
{
    public sealed class EncryptFilePathStep : Step
    {
        public EncryptFilePathStep() : base(isAsync: false) { }
        internal override bool Method(ref IngredientModel ingredients) => EncryptFilePath(ref ingredients);
        internal override Task<bool> MethodAsync(ref IngredientModel ingredients) => throw new NotImplementedException();

        private static bool EncryptFilePath(ref IngredientModel ingredients)
        {
            if (!ingredients.IsValid() || !Config.Encryption.IsValid()) return false.Trail($"{nameof(EncryptFilePathStep)} validation failed.");
            var isValid = false;
            byte[] cipherbytes = null;

            try
            {
                cipherbytes = Aes.Encrypt(Config.Encryption.Password, Encoding.UTF8.GetBytes(ingredients.Path));
                ingredients = ingredients.WithPath(Encoding.UTF8.GetString(cipherbytes));

                cipherbytes = Aes.Encrypt(Config.Encryption.Password, Encoding.UTF8.GetBytes(ingredients.Name));
                ingredients = ingredients.WithName(Encoding.UTF8.GetString(cipherbytes));

                cipherbytes = Aes.Encrypt(Config.Encryption.Password, Encoding.UTF8.GetBytes(ingredients.Extension));
                ingredients = ingredients.WithExtension(Encoding.UTF8.GetString(cipherbytes));

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

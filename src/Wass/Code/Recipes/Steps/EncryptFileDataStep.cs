using Wass.Code.Encryption;
using Wass.Code.Infrastructure;
using Wass.Code.Persistence.Configuration;

namespace Wass.Code.Recipes.Steps
{
    public sealed class EncryptFileDataStep : Step
    {
        public EncryptFileDataStep() : base(isAsync: false) { }
        internal override Task<bool> MethodAsync(ref IngredientModel ingredients) => throw new NotImplementedException();
        internal override bool Method(ref IngredientModel ingredients) => EncryptFileData(ref ingredients);

        private static bool EncryptFileData(ref IngredientModel ingredients)
        {
            if (!ingredients.IsValid() || !Config.Encryption.IsValid()) return false.Trail($"{nameof(EncryptFileDataStep)} validation failed.");
            var isValid = false;

            try
            {
                var cipherbytes = Aes.Encrypt(Config.Encryption.Password, ingredients.Data);
                ingredients = ingredients.WithData(cipherbytes);
                isValid = true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, nameof(EncryptFileDataStep));
            }

            return isValid;
        }
    }
}

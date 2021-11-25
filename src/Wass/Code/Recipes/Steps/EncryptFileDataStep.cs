using System.Collections.Specialized;
using Wass.Code.Encryption;
using Wass.Code.Infrastructure;
using Wass.Code.Persistence.Configuration;

namespace Wass.Code.Recipes.Steps
{
    public sealed class EncryptFileDataStep : Step
    {
        public EncryptFileDataStep() : base(isAsync: false) { }
        internal override bool Method(ref FileModel file, ListDictionary ingredients) => EncryptFileData(ref file, ingredients);
        internal override Task<bool> MethodAsync(ref FileModel file, ListDictionary ingredients) => throw new NotImplementedException();

        private static bool EncryptFileData(ref FileModel file, ListDictionary ingredients)
        {
            if (!file.IsValid() || !Config.Encryption.IsValid()) return false.Trail($"{nameof(EncryptFileDataStep)} validation failed.");
            var isValid = false;

            try
            {
                var cipherbytes = Aes.Encrypt(Config.Encryption.Password, file.Data);
                file = file.WithData(cipherbytes);
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

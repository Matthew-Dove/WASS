using ContainerExpressions.Containers;

namespace Wass.Core.Models.Options
{
    public class EncryptionOptions : SmartEnum
    {
        public static readonly EncryptionOptions Aes = new();

        private EncryptionOptions() { }
    }
}

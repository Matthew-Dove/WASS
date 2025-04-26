using ContainerExpressions.Containers;

namespace Wass.Core.Models.Options
{
    public class EncryptionOptions : SmartEnum
    {
        public static readonly EncryptionOptions None = new(1);
        public static readonly EncryptionOptions Aes = new(2);

        private EncryptionOptions(int value) : base(value) { }
    }
}

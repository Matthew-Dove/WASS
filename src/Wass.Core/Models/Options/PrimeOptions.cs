using ContainerExpressions.Containers;

namespace Wass.Core.Models.Options
{
    /// <summary>"Priming the pump" is a technique for warming up code on startup.</summary>
    public static class PrimeOptions
    {
        /// <summary>Run the static constructors for each SmartEnum type, so the properites can be accessed safely.</summary>
        public static void ThePump()
        {
            _ = SmartEnum<CompressionOptions>.Init();
            _ = SmartEnum<EncryptionOptions>.Init();
            _ = SmartEnum<ResourceOptions>.Init();
        }
    }
}

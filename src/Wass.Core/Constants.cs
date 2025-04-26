namespace Wass.Core
{
    /// <summary>Constants class.</summary>
    internal static class C
    {
        /// <summary>OPSEC: Operational Security - minimum length for salt byte arrays; and string password characters.</summary>
        public const int OpSecSize = 16;

        /// <summary>OPSEC: Number of iterations to make for encryption, and hashing algorithms.</summary>
        public const int OpSecIterations = 666666;

        /// <summary>
        /// The overall version for WASS, for things like folder structures, and file formats.
        /// <para>Generally the version will only get bumped for breaking, or incompatible changes.</para>
        /// </summary>
        public const string Version = "1";
    }
}

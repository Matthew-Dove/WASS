using FrameworkContainers.Format.JsonCollective.Models;

namespace Wass.Core
{
    /// <summary>Constants class.</summary>
    public static class C
    {
        /// <summary>OPSEC: Operational Security - minimum length for salt byte arrays; and string password characters.</summary>
        public const int OpSecSize = 16; // 16 is fine to use, but 32 is recommend as the gold standard.

        /// <summary>OPSEC: Number of iterations to make for encryption, and hashing algorithms.</summary>
        public const int OpSecIterations = 666666;

        /// <summary>
        /// The overall version for WASS, for things like folder structures, and file formats.
        /// <para>Generally the version will only get bumped for breaking, or incompatible changes.</para>
        /// </summary>
        public const string Version = "1";

        /// <summary>Json options to use when serialising to / from models.</summary>
        public static JsonOptions JsonOptions = JsonOptions.PermissiveCamelCase;
    }
}

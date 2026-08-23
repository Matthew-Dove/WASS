namespace Wass.Core.Models.Upload
{
    public sealed class TagMetadataModel
    {
        /// <summary>
        /// When encryption is used, Noise adds some random bytes to the model.
        /// <para>Used to make reverse engineering on small known data sets harder.</para>
        /// <para>Can be null / empty if not used.</para>
        /// </summary>
        public string Noise { get; set; }

        /// <summary>ISO 8601.</summary>
        public string Created { get; set; }
    }
}

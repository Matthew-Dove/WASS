using Wass.Code.Infrastructure;

namespace Wass.Code.Recipes
{
    internal sealed class FileModel
    {
        /// <summary>The file's data (raw bytes may have been transformed e.g. compressed / encrypted.</summary>
        public byte[] Data { get; private set; }

        /// <summary>The file's full path (directory + name + extension).</summary>
        public string Path { get; set; }

        public FileModel(byte[] data, string path)
        {
            if (data == null || data.Length < 1) throw new ArgumentException("Length must be greater than 0.", nameof(data));
            if (string.IsNullOrEmpty(path)) throw new ArgumentException("Length must be greater than 0.", nameof(path));
            if (!path.TrySplitPath(out var sp) || (string.IsNullOrEmpty(sp.Item1) || string.IsNullOrEmpty(sp.Item2) || string.IsNullOrEmpty(sp.Item3))) throw new ArgumentException("Must contain file's directory, name, and extension.", nameof(path));

            Data = data;
            Path = path;
        }

        public FileModel WithData(byte[] data) { Data = data; return this; }
        public FileModel WithPath(string path) { Path = path; return this; }
    }

    internal static class FileModelExtensions
    {
        public static bool IsValid(this FileModel file)
        {
            return
                file != null &&
                file.Data.Length > 0 &&
                !string.IsNullOrEmpty(file.Path);
        }

        public static bool TrySplitPath(this string path, out (string, string, string) splitPath)
        {
            var isValid = false;
            splitPath = default;

            try
            {
                string
                    directory = Path.GetDirectoryName(path),
                    name = Path.GetFileNameWithoutExtension(path),
                    extension = Path.GetExtension(path);

                if (extension != null && extension.StartsWith('.')) extension = extension[1..];

                if (!string.IsNullOrEmpty(directory) && !string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(extension))
                {
                    splitPath = (directory, name, extension);
                    isValid = true;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error splitting file path into directory, name, and extension: [{path}].");
            }

            return isValid.Trail(x => $"Was {nameof(TrySplitPath)} from {nameof(FileModelExtensions)} successful: {x}.");
        }
    }
}

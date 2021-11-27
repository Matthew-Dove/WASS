using Wass.Code.Infrastructure;

namespace Wass.Code.Recipes
{
    internal sealed class FileModel
    {
        /// <summary>The file's data (raw bytes may have been transformed e.g. compressed / encrypted.</summary>
        public byte[] Data { get; private set; }

        /// <summary>The file's directory: "C:\backup\docs".</summary>
        public string Directory { get; private set; }

        /// <summary>The file's name: "birthdays".</summary>
        public string Name { get; private set; }

        /// <summary>The file's extension: "txt".</summary>
        public string Extension { get; private set; }

        public FileModel(byte[] data, string directory, string name, string extension)
        {
            Data = data;
            Directory = directory;
            Name = name;
            Extension = extension;
        }

        public FileModel WithData(byte[] data) { Data = data; return this; }
        public FileModel WithDirectory(string directory) { Directory = directory; return this; }
        public FileModel WithName(string name) { Name = name; return this; }
        public FileModel WithExtension(string extension) { Extension = extension; return this; }
    }

    internal static class FileModelExtensions
    {
        public static bool IsValid(this FileModel file)
        {
            return
                file != null &&
                file.Data != null &&
                file.Data.Length > 0 &&
                !string.IsNullOrEmpty(file.Directory) &&
                !string.IsNullOrEmpty(file.Name) &&
                !string.IsNullOrEmpty(file.Extension);
        }

        public static string GetPath(this FileModel file) => file.Directory + Path.DirectorySeparatorChar + file.Name + "." + file.Extension;

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

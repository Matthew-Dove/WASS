using Wass.Code.Infrastructure;

namespace Wass.Code.Recipes
{
    internal readonly struct FileModel
    {
        /// <summary>The file's data (raw bytes may have been transformed e.g. compressed / encrypted.</summary>
        public byte[] Data { get; }

        /// <summary>The file's relative location: "/backup/docs".</summary>
        public string Location { get; }

        /// <summary>The file's name: "birthdays".</summary>
        public string Name { get; }

        /// <summary>The file's extension: "txt".</summary>
        public string Extension { get; }

        public FileModel(byte[] data, string location, string name, string extension)
        {
            Data = data;
            Location = location;
            Name = name;
            Extension = extension;
        }
    }

    internal static class FileModelExtensions
    {
        public static FileModel WithData(this FileModel file, byte[] data) => new(data, file.Location, file.Name, file.Extension);
        public static FileModel WithLocation(this FileModel file, string location) => new (file.Data, location, file.Name, file.Extension);
        public static FileModel WithName(this FileModel file, string name) => new (file.Data, file.Location, name, file.Extension);
        public static FileModel WithExtension(this FileModel file, string extension) => new (file.Data, file.Location, file.Name, extension);

        public static bool IsValid(this FileModel file)
        {
            return
                file.Data != null &&
                file.Data.Length > 0 &&
                !string.IsNullOrEmpty(file.Location) &&
                !string.IsNullOrEmpty(file.Name) &&
                !string.IsNullOrEmpty(file.Extension);
        }

        public static string GetPath(this FileModel file) => file.Location + Path.DirectorySeparatorChar + file.Name + "." + file.Extension;

        public static bool TrySplitPath(this string path, out (string, string, string) splitPath)
        {
            var isValid = false;
            splitPath = default;

            try
            {
                string
                    location = Path.GetDirectoryName(path),
                    name = Path.GetFileNameWithoutExtension(path),
                    extension = Path.GetExtension(path);

                if (extension != null && extension.StartsWith('.')) extension = extension[1..];

                if (!string.IsNullOrEmpty(location) && !string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(extension))
                {
                    splitPath = (location, name, extension);
                    isValid = true;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error splitting file path into location, name, and extension.");
            }

            return isValid;
        }
    }
}

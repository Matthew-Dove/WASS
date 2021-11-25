namespace Wass.Code.Recipes
{
    internal readonly struct FileModel
    {
        /// <summary>The file's data (raw bytes may have been transformed e.g. compressed / encrypted.).</summary>
        public byte[] Data { get; }

        /// <summary>The file's path: "C:\personal\docs".</summary>
        public string Path { get; }

        /// <summary>The file's name: "birthdays".</summary>
        public string Name { get; }

        /// <summary>The file's extension: "txt".</summary>
        public string Extension { get; }

        public FileModel(byte[] data, string path, string name, string extension)
        {
            Data = data;
            Path = path;
            Name = name;
            Extension = extension;
        }
    }

    internal static class FiletModelExtensions
    {
        public static FileModel WithData(this FileModel file, byte[] data) => new(data, file.Path, file.Name, file.Extension);
        public static FileModel WithPath(this FileModel file, string path) => new (file.Data, path, file.Name, file.Extension);
        public static FileModel WithName(this FileModel file, string name) => new (file.Data, file.Path, name, file.Extension);
        public static FileModel WithExtension(this FileModel file, string extension) => new (file.Data, file.Path, file.Name, extension);

        public static bool IsValid(this FileModel file)
        {
            return
                file.Data != null &&
                file.Data.Length > 0 &&
                !string.IsNullOrEmpty(file.Path) &&
                !string.IsNullOrEmpty(file.Name) &&
                !string.IsNullOrEmpty(file.Extension);
        }
    }
}

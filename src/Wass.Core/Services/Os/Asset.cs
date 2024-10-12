using ContainerExpressions.Containers;

namespace Wass.Core.Services.Os
{
    public interface IAsset
    {

    }

    public sealed class Asset : IAsset
    {
        
    }

    /// <summary>The unit to use when calculating the filesize, one of: B | KB | MB | GB.</summary>
    public sealed class UnitVariant(string unit) :
        Alias<string>(unit.ThrowIf(static x => !_units.Contains(x), "Unit [{Unit}] is not valid for the filesize.".WithArgs(unit))) {
        private static readonly string[] _units = ["B", "KB", "MB", "GB"];
    }

    public static class FileModelExtensions
    {
        public static int GetFileSize(this byte[] data, UnitVariant unit)
        {
            const int
                B = 1,
                KB = 1024,
                MB = 1024 * 1024,
                GB = 1024 * 1024 * 1024;

            var unitMultiplier = 0;

            if (unit.Equals("B")) unitMultiplier = B;
            if (unit.Equals("KB")) unitMultiplier = KB;
            if (unit.Equals("MB")) unitMultiplier = MB;
            if (unit.Equals("GB")) unitMultiplier = GB;

            return data.Length * unitMultiplier;
        }

        /// <summary>Removes the path root, and sets directory seperators to "/".</summary>
        public static string GetNormalisedPath(this string path)
        {
            path = path.Substring(Path.GetPathRoot(path).Length).Replace('\\', '/');
            if (path.StartsWith("./")) path = path.Substring("./".Length);
            if (path.StartsWith("/")) path = path.Substring("/".Length);
            return path;
        }

        public static Response<(string Directory, string Name, string Extension)> SplitPath(this string path)
        {
            var response = new Response<(string, string, string)>();

            try
            {
                string
                    directory = Path.GetDirectoryName(path),
                    name = Path.GetFileNameWithoutExtension(path),
                    extension = Path.GetExtension(path);

                if (!string.IsNullOrEmpty(directory) && !string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(extension))
                {
                    response = response.With((directory, name, extension));
                }
            }
            catch (Exception ex)
            {
                ex.LogError("Error splitting file path into directory, name, and extension for path [{Path}].".WithArgs(path));
            }

            return response;
        }
    }
}

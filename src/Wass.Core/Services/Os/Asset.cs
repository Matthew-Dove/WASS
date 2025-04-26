using ContainerExpressions.Containers;

namespace Wass.Core.Services.Os
{
    public interface IAsset
    {
        Response<bool> Exists(string file);
        Response<double> Size(string file, UnitVariant unit);
        Task<Response<byte[]>> Load(string file);
        (string Directory, string Name, string Extension) SplitPath(string path);
    }

    public sealed class Asset : IAsset
    {
        public Response<bool> Exists(string file) => Try.Run(() => File.Exists(file), "Error checking if file: [{File}] exists.".WithArgs(file));

        public Response<double> Size(string file, UnitVariant unit)
        {
            return Try.Run(() =>
            {
                var size = 0D;
                var fileInfo = new FileInfo(file);
                if (fileInfo.Exists)
                {
                    size = fileInfo.GetFileSize(unit);
                }
                return size;
            }, "Error getting the file size for: [{file}].".WithArgs(file));
        }

        public Task<Response<byte[]>> Load(string file) => Try.RunAsync(async () => await File.ReadAllBytesAsync(file), "Error loading file: [{File}].".WithArgs(file));

        public (string Directory, string Name, string Extension) SplitPath(string path)
        {
            path = Path.GetFullPath(path);

            string
                directory = Path.GetDirectoryName(path),
                name = Path.GetFileNameWithoutExtension(path),
                extension = Path.GetExtension(path);

            directory = directory.GetNormalisedPath();
            return (directory, name, extension);
        }
    }

    public enum UnitVariant { B, KB, MB, GB }

    file static class AssetExtensions
    {
        public static double GetFileSize(this FileInfo fileInfo, UnitVariant unit) => GetFileSize(fileInfo.Length, unit);
        public static double GetFileSize(this byte[] data, UnitVariant unit) => GetFileSize(data.LongLength, unit);
        private static double GetFileSize(long fileLength, UnitVariant unit)
        {
            const long
                B = 1L,
                KB = 1024L,
                MB = 1024L * 1024L,
                GB = 1024L * 1024L * 1024L;

            double unitMultiplier = unit switch
            {
                UnitVariant.B => B,
                UnitVariant.KB => KB,
                UnitVariant.MB => MB,
                UnitVariant.GB => GB,
                _ => Lambda.Throw<long>(new ArgumentOutOfRangeException(nameof(unit), $"Invalid unit: {unit}."))
            };

            return fileLength / unitMultiplier;
        }

        /// <summary>Removes the path root, and sets directory seperators to "/".</summary>
        public static string GetNormalisedPath(this string path)
        {
            path = path.Substring(Path.GetPathRoot(path).Length).Replace('\\', '/');
            if (path.StartsWith("./")) path = path.Substring("./".Length);
            if (path.StartsWith("/")) path = path.Substring("/".Length);
            return path;
        }
    }
}

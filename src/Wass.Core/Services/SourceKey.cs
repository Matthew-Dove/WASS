using ContainerExpressions.Containers;
using Wass.Core.Models.Options;
using Wass.Core.Services.Encryption;

namespace Wass.Core.Services
{
    /// <summary>Holds common key path functions.</summary>
    file static class Key
    {
        public const string FileObjectName = "file.wass.bin";
        public const string MetadataObjectName = "metadata.wass.bin";
        public const string ConfigObjectName = "config.wass.bin";
        public const string TemplateKeyPrefix = "templates";
        public const string TemplateConfigName = "config.wass.template.bin";

        public const string SchemaDirectory = "\\schemas\\";
        public const string TemplateDirectory = "\\" + TemplateKeyPrefix + "\\";

        public static string GetResource(ResourceOptions resource)
        {
            return resource.Name switch
            {
                nameof(ResourceOptions.Files) => "files",
                nameof(ResourceOptions.Tags) => "tags",
                _ => Lambda.Throw<string>(new ArgumentOutOfRangeException(nameof(resource), $"Resource: \"{resource}\", does not have a mapping to a lowercase variant."))
            };
        }
    }

    /// <summary>A utility class to create source key paths for file objects (i.e. on S3).</summary>
    public static class SourceKey
    {
        public static string GetConfigTemplatePath(string templateHash, string version)
        {
            return $"{Key.TemplateKeyPrefix}/{templateHash}/v{version}/{Key.TemplateConfigName}";
        }

        public static string GetConfigPath(Either<string, byte[]> fileHash, string version, ResourceOptions resource)
        {
            return $"{GetBasePath(fileHash, version, resource)}{Key.ConfigObjectName}";
        }

        public static string GetMetadataPath(Either<string, byte[]> fileHash, string version, ResourceOptions resource)
        {
            return $"{GetBasePath(fileHash, version, resource)}{Key.MetadataObjectName}";
        }

        public static string GetFilePath(Either<string, byte[]> fileHash, string version, ResourceOptions resource)
        {
            return $"{GetBasePath(fileHash, version, resource)}{Key.FileObjectName}";
        }

        private static string GetBasePath(Either<string, byte[]> fileHash, string version, ResourceOptions resource)
        {
            return $"{GetFileHex(fileHash)}/v{version}/{Key.GetResource(resource)}/";
        }

        private static string GetFileHex(Either<string, byte[]> fileHash) => fileHash.Match(Lambda.Identity, static x => x.BytesToHex());
    }

    /// <summary>A utility class to create local key paths for files (i.e. on drive / nas).</summary>
    public static class FileKey
    {
        public static string GetConfigPath(string localRoot, string sourcePath)
        {
            var rootPath = GetBasePath(localRoot, Key.SchemaDirectory);
            var relativePath = sourcePath[..^Key.ConfigObjectName.Length] + "config.json";
            return Path.GetFullPath(Path.Join(rootPath, relativePath));
        }

        public static string GetMetadataPath(string localRoot, string sourcePath)
        {
            var rootPath = GetBasePath(localRoot, Key.SchemaDirectory);
            var relativePath = sourcePath[..^Key.MetadataObjectName.Length] + "metadata.json";
            return Path.GetFullPath(Path.Join(rootPath, relativePath));
        }

        public static string GetFilePath(string localRoot, string sourcePath)
        {
            return Path.GetFullPath(Path.Join(localRoot, sourcePath));
        }

        private static string GetBasePath(string localRoot, string directory)
        {
            return Path.GetFullPath(Path.Join(localRoot, directory));
        }
    }
}

using Amazon.S3;
using ContainerExpressions.Containers;
using FrameworkContainers.Format.JsonCollective;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Wass.Core.Models;
using Wass.Core.Models.Configuration;
using Wass.Core.Models.Options;
using Wass.Core.Models.Upload;
using Wass.Core.Services.Compression;
using Wass.Core.Services.Encryption;
using Wass.Core.Services.Os;
using Wass.Core.Services.Persistence;

namespace Wass.Core.Services.Actions
{
    public interface IBackupAction
    {
        Task<Response<Unit>> Backup(ActionRequest request);
    }

    public sealed class BackupAction(
        IS3 _s3,
        IOptions<DestinationConfig> _config,
        IOptions<SecurityConfig> _security,
        IAsset _asset,
        IBrotli _brotli,
        IGZip _gzip,
        IAes _aes,
        IHash _hash
        ) : IBackupAction
    {
        public async Task<Response<Unit>> Backup(ActionRequest request)
        {
            var config = _config.Value.Sources[request.Source];
            Log.Info("Executing Backup command for source: \"{Source}\" on file: \"{File}\".".WithArgs(request.Source, request.File));

            var bucketExists = await _s3.DoesBucketExist(request.Source, config.Bucket);
            if (bucketExists.IsTrue(x => !x))
            {
                var createBucket = await _s3.CreateBucket(request.Source, config.Bucket);
                bucketExists = createBucket.Transform(_ => true);
            }

            var data = await bucketExists.BindIfAsync(Lambda.Identity, _ => _asset.Load(request.File));
            return await data.BindAsync(x => ApplyFileOptions(request, x));
        }

        private async Task<Response<Unit>> ApplyFileOptions(ActionRequest request, byte[] data)
        {
            Log.Info("File size: {FileSize}KB, is compression enabled: {Compression}, is encryption enabled: {Encryption}.".WithArgs(data.Length / 1024, !request.Compression.HasFlag(CompressionOptions.None), !request.Encryption.HasFlag(EncryptionOptions.None)));

            // Hash the file contents before any compression, or encryption is applied; otherwise the hash will be different each time.
            var salt = string.IsNullOrEmpty(_security.Value.Salt) ? null : _security.Value.Salt.HexToBytes();
            var fileHash = salt == null ? _hash.ComputeHash(data) : _hash.ComputeHash(data, salt);

            var fileParts = _asset.SplitPath(request.File);
            var shouldCompress = !Category.IsCompressed(fileParts.Extension).LogValue(x => "Is the file extension {Extension} already compressed: {IsCompressed}.".WithArgs(fileParts.Extension, x));
            var stream = CompressAndEncrypt(request, data, shouldCompress: shouldCompress);

            return await stream.BindAsync(x => ApplyMetadataOptions(request, fileHash, x, $"{fileParts.Directory}/{fileParts.Name}{fileParts.Extension}"));
        }

        private async Task<Response<Unit>> ApplyMetadataOptions(ActionRequest request, byte[] fileHash, byte[] fileStream, string path)
        {
            var resource = SmartEnum<ResourceOptions>.FromObject(ResourceOptions.File).Value;

            var metadataModel = new FileMetadataModel { Created = DateTime.UtcNow.ToIso8601(), Path = path };
            var configModel = new WassConfigModel
            {
                EncryptionKeyId = _security.Value.PasswordKeyId,
                HashKeyId = _security.Value.SaltKeyId,
                Version = C.Version,
                Resource = resource,
                Encryption = request.Encryption,
                Compression = request.Compression
            };

            var metadataJson = Json.FromModel(metadataModel);
            var configJson = Json.FromModel(configModel);

            var metadataStream = CompressAndEncrypt(request, metadataJson.Utf8ToBytes(), unit: UnitVariant.B);
            var configStream = CompressAndEncrypt(request, configJson.Utf8ToBytes(), unit: UnitVariant.B);

            return await Upload(request, fileHash, fileStream, metadataStream, configStream);
        }

        private async Task<Response<Unit>> Upload(ActionRequest request, byte[] fileHash, byte[] fileStream, byte[] metadataStream, byte[] configStream)
        {
            var config = _config.Value.Sources[request.Source];
            var metadataPath = $"{fileHash.BytesToHex()}/v{C.Version}/metadata.wass.bin".ToLowerInvariant();
            var configPath = $"{fileHash.BytesToHex()}/v{C.Version}/config.wass.json".ToLowerInvariant();
            var filePath = $"{fileHash.BytesToHex()}/v{C.Version}/file.wass.bin".ToLowerInvariant();

            // Upload metadata first (to catch any issues early), then the request's config, and lastly the actual file.
            var metadataExists = await _s3.DoesFileExist(request.Source, config.Bucket, metadataPath);
            if (metadataExists.IsTrue(x => !x))
            {
                var createFile = await _s3.CreateFile(request.Source, config.Bucket, metadataPath, metadataStream, S3StorageClass.IntelligentTiering);
                metadataExists = createFile.Transform(_ => true);
            }

            var configExists = await metadataExists.BindIfAsync(Lambda.Identity, _ => _s3.DoesFileExist(request.Source, config.Bucket, configPath));
            if (configExists.IsTrue(x => !x))
            {
                var createFile = await _s3.CreateFile(request.Source, config.Bucket, configPath, configStream, S3StorageClass.IntelligentTiering);
                configExists = createFile.Transform(_ => true);
            }

            var fileExists = await configExists.BindIfAsync(Lambda.Identity, _ => _s3.DoesFileExist(request.Source, config.Bucket, filePath));
            if (fileExists.IsTrue(x => !x))
            {
                var createFile = await _s3.CreateFile(request.Source, config.Bucket, filePath, fileStream, S3StorageClass.IntelligentTiering);
                fileExists = createFile.Transform(_ => true);
            }

            return fileExists
                .Validate(Lambda.Identity)
                .Transform(_ => Unit.Instance)
                .Log("Successfully backed up file: \"{Path}\".".WithArgs(request.File), "Failed attempting to back up the file: \"{Path}\".".WithArgs(request.File));
        }

        private Response<byte[]> CompressAndEncrypt(ActionRequest request, byte[] data, UnitVariant unit = UnitVariant.KB, bool shouldCompress = true)
        {
            var stream = Response.Create(data);
            var unitDisplay = unit.ToString();
            var divisor = unit switch  {
                UnitVariant.KB => 1024,
                UnitVariant.MB => 1024 * 1024,
                UnitVariant.GB => 1024 * 1024 * 1024,
                _ => 1
            };
            if (stream && shouldCompress && request.Compression.HasFlag(CompressionOptions.Brotli)) stream = _brotli.Compress(data).Log(x => "Brotli compression reduced size from {DataSize}{DataUnit} to {CompressedSize}{CompressedUnit}.".WithArgs(data.Length / divisor, unitDisplay, x.Length / divisor, unitDisplay));
            if (stream && shouldCompress && request.Compression.HasFlag(CompressionOptions.GZip)) stream = _gzip.Compress(data).Log(x => "GZip compression reduced size from {DataSize}{DataUnit} to {CompressedSize}{CompressedUnit}.".WithArgs(data.Length / divisor, unitDisplay, x.Length / divisor, unitDisplay));
            if (stream && request.Encryption.HasFlag(EncryptionOptions.Aes)) stream = _aes.Encrypt(_security.Value.Password, stream).Log("Data encrypted with AES.");
            return stream;
        }
    }
}

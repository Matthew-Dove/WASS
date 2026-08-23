using Amazon.S3;
using ContainerExpressions.Containers;
using FrameworkContainers.Format.JsonCollective;
using FrameworkContainers.Format.JsonCollective.Models;
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
            var stream = CompressAndEncrypt(request, data, UnitVariant.KB, shouldCompress);

            return await stream.BindAsync(x => ApplyMetadataOptions(request, fileHash, x, $"{fileParts.Directory}/{fileParts.Name}{fileParts.Extension}", shouldCompress));
        }

        private async Task<Response<Unit>> ApplyMetadataOptions(ActionRequest request, byte[] fileHash, byte[] fileStream, string path, bool shouldCompress)
        {
            var noise = request.Encryption == EncryptionOptions.None ? null : _hash.ComputeHash(path.Utf8ToBytes(), _hash.GenerateSalt(C.OpSecSize)).BytesToBase64();
            var metadataModel = new FileMetadataModel { Noise = noise, Created = DateTime.UtcNow.ToIso8601(), Path = path, IsCompressible = shouldCompress };
            var configModel = new WassConfigModel
            {
                EncryptionKeyId = _security.Value.PasswordKeyId,
                HashKeyId = _security.Value.SaltKeyId,
                Version = C.Version,
                Resource = ResourceOptions.Files,
                Encryption = request.Encryption,
                Compression = request.Compression
            };

            var configJson = Json.FromModel(configModel, C.JsonOptions);
            var metadataJson = Json.FromModel(metadataModel, C.JsonOptions);

            var metadataStream = CompressAndEncrypt(request, metadataJson.Utf8ToBytes(), UnitVariant.B, shouldCompress: false);
            var configStream = configJson.Utf8ToBytes(); // Config is not encrypted, don't include file info/metadata on this model.

            return await metadataStream.BindAsync(x => Upload(request, fileHash, fileStream, x, configStream));
        }

        private async Task<Response<Unit>> Upload(ActionRequest request, byte[] fileHash, byte[] fileStream, byte[] metadataStream, byte[] configStream)
        {
            var config = _config.Value.Sources[request.Source];
            var fileHex = fileHash.BytesToHex();

            var configPath = SourceKey.GetConfigPath(fileHex, C.Version, ResourceOptions.Files);
            var metadataPath = SourceKey.GetMetadataPath(fileHex, C.Version, ResourceOptions.Files);
            var filePath = SourceKey.GetFilePath(fileHex, C.Version, ResourceOptions.Files);

            // Upload config first (to catch any issues early).
            var configExists = await  _s3.DoesFileExist(request.Source, config.Bucket, configPath);
            if (configExists.IsTrue(x => !x))
            {
                var createFile = await _s3.CreateFile(request.Source, config.Bucket, configPath, configStream, S3StorageClass.IntelligentTiering);
                configExists = createFile.Transform(static _ => true);
            }

            var metadataExists = await configExists.BindIfAsync(Lambda.Identity, _ => _s3.DoesFileExist(request.Source, config.Bucket, metadataPath));
            if (metadataExists.IsTrue(x => !x))
            {
                var createFile = await _s3.CreateFile(request.Source, config.Bucket, metadataPath, metadataStream, S3StorageClass.IntelligentTiering);
                metadataExists = createFile.Transform(static _ => true);
            }
            
            var fileExists = await metadataExists.BindIfAsync(Lambda.Identity, _ => _s3.DoesFileExist(request.Source, config.Bucket, filePath));
            if (fileExists.IsTrue(x => !x))
            {
                var createFile = await _s3.CreateFile(request.Source, config.Bucket, filePath, fileStream, S3StorageClass.IntelligentTiering);
                fileExists = createFile.Transform(static _ => true);
            }

            return fileExists
                .Validate(Lambda.Identity)
                .Transform(_ => Unit.Instance)
                .Log("Successfully backed up file: \"{Path}\".".WithArgs(request.File), "Failed attempting to back up the file: \"{Path}\".".WithArgs(request.File));
        }

        private Response<byte[]> CompressAndEncrypt(ActionRequest request, byte[] data, UnitVariant unit, bool shouldCompress)
        {
            var stream = Response.Create(data);
            var unitDisplay = unit.ToString();
            var divisor = unit switch  {
                UnitVariant.KB => 1024,
                UnitVariant.MB => 1024 * 1024,
                UnitVariant.GB => 1024 * 1024 * 1024,
                _ => 1
            };
            if (stream && shouldCompress && request.Compression.HasFlag(CompressionOptions.Brotli)) stream = _brotli.Compress(stream).Log(x => "Brotli compression reduced size from {DataSize}{DataUnit} to {CompressedSize}{CompressedUnit}.".WithArgs(data.Length / divisor, unitDisplay, x.Length / divisor, unitDisplay));
            if (stream && shouldCompress && request.Compression.HasFlag(CompressionOptions.GZip)) stream = _gzip.Compress(stream).Log(x => "GZip compression reduced size from {DataSize}{DataUnit} to {CompressedSize}{CompressedUnit}.".WithArgs(data.Length / divisor, unitDisplay, x.Length / divisor, unitDisplay));
            if (stream && request.Encryption.HasFlag(EncryptionOptions.Aes)) stream = _aes.Encrypt(_security.Value.Password, stream).Log("Data encrypted with AES.");
            return stream;
        }
    }
}

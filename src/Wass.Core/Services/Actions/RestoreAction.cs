using ContainerExpressions.Containers;
using ContainerExpressions.Expressions;
using FrameworkContainers.Format.JsonCollective;
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
    sealed class JsonFormat : Alias<string> { public JsonFormat(string value) : base(value) { } }

    public interface IRestoreAction
    {
        Task<Response<Unit>> Restore(ActionRequest request);
    }

    public sealed class RestoreAction(
        IS3 _s3,
        IOptions<DestinationConfig> _config,
        IOptions<SecurityConfig> _security,
        IOptions<DownloadConfig> _download,
        IAsset _asset,
        IBrotli _brotli,
        IGZip _gzip,
        IAes _aes,
        IDryRunAction _dryRun
        ) : IRestoreAction
    {
        public async Task<Response<Unit>> Restore(ActionRequest request)
        {
            if (request.IsDryRun) await _dryRun.DryRun(request);
            var config = _config.Value.Sources[request.Source];
            Log.Info("Executing Restore command for source: \"{Source}\".".WithArgs(request.Source));

            var bucketExists = await _s3.DoesBucketExist(request.Source, config.Bucket);
            if (bucketExists.IsTrue(x => !x)) return new Response<Unit>().LogErrorValue("The bucket: \"{Bucket}\" does not exist.".WithArgs(config.Bucket));
            if (C.Version != "1") return new Response<Unit>().LogErrorValue("The version: \"{Version}\" is not known.".WithArgs(C.Version));

            var filePath = SourceKey.GetFilePath(request.FileHash, C.Version, ResourceOptions.Files);
            var fileExists = await bucketExists.BindAsync(_ => _s3.DoesFileExist(request.Source, config.Bucket, filePath));
            if (fileExists.IsTrue(x => !x)) return new Response<Unit>().LogErrorValue("The S3 key: \"{Key}\" does not exist.".WithArgs(filePath));

            return await fileExists.BindAsync(_ => DownloadFile(request));
        }

        private async Task<Response<Unit>> DownloadFile(ActionRequest request)
        {
            var resposne = new Response<Unit>();
            var config = _config.Value.Sources[request.Source];

            var configPath = SourceKey.GetConfigPath(request.FileHash, C.Version, ResourceOptions.Files);
            var metadataPath = SourceKey.GetMetadataPath(request.FileHash, C.Version, ResourceOptions.Files);
            var filePath = SourceKey.GetFilePath(request.FileHash, C.Version, ResourceOptions.Files);

            var configExists = _s3.DoesFileExist(request.Source, config.Bucket, configPath);
            var metadataExists = _s3.DoesFileExist(request.Source, config.Bucket, metadataPath);
            var fileExists = _s3.DoesFileExist(request.Source, config.Bucket, filePath);

            var allExist = await Expression.FunnelAsync(metadataExists, configExists, fileExists, static (x, y, z) => x && y && z).LogAsync(x => "All related files found for restore: {IsFound}.".WithArgs(x));

            if (allExist.IsTrue(Lambda.Identity))
            {
                var configBlob = _s3.DownloadFile(request.Source, config.Bucket, configPath);
                var metadataBlob = _s3.DownloadFile(request.Source, config.Bucket, metadataPath);
                var fileBlob = _s3.DownloadFile(request.Source, config.Bucket, filePath);

                var blobs = Expression.FunnelAsync(configBlob, metadataBlob, fileBlob, DecodeBlobs);
                var isSaved = await blobs.BindAsync(x => Save(configPath, metadataPath, x.FilePath, x.Config, x.Metadata, x.FileData));
            }

            return resposne;
        }
        
        private Response<(JsonFormat Config, JsonFormat Metadata, string FilePath, byte[] FileData)> DecodeBlobs(byte[] configBlob, byte[] metadataBlob, byte[] fileBlob)
        {
            var response = new Response<(JsonFormat, JsonFormat, string, byte[])>();
            var none = SmartEnum<CompressionOptions>.FromObject(CompressionOptions.None);

            var configJson = new JsonFormat(configBlob.BytesToUtf8());
            var config = Json.Response.ToModel<WassConfigModel>(configJson, C.JsonOptions).Validate(x => x?.Version == C.Version);
            if (config)
            {
                var compression = SmartEnum<CompressionOptions>.FromName(config.Value.Compression);
                var encryption = SmartEnum<EncryptionOptions>.FromName(config.Value.Encryption);
                if (compression && encryption)
                {
                    var metadataBytes = DecryptAndDecompress(metadataBlob, encryption, none);
                    var metadataJson = metadataBytes.Transform(x => new JsonFormat(x.BytesToUtf8()));
                    var metadata = metadataJson.Bind(x => Json.Response.ToModel<FileMetadataModel>(x, C.JsonOptions)).Validate(x => !string.IsNullOrEmpty(x.Created));
                    var fileData = metadata.Bind(x => DecryptAndDecompress(fileBlob, encryption, x.IsCompressible ? compression : none));
                    response = fileData.Transform(_ => response.With((configJson, metadataJson, metadata.Value.Path, fileData)));
                }
            }

            return response;
        }

        private Response<byte[]> DecryptAndDecompress(byte[] data, EnumRange<EncryptionOptions> encryption, EnumRange<CompressionOptions> compression)
        {
            var stream = Response.Create(data);

            if (stream && encryption.HasFlag(EncryptionOptions.Aes)) stream = _aes.Decrypt(_security.Value.Password, stream);
            if (stream && compression.HasFlag(CompressionOptions.Brotli)) stream = _brotli.Decompress(stream);
            if (stream && compression.HasFlag(CompressionOptions.GZip)) stream = _gzip.Decompress(stream);

            return stream;
        }

        /**
         * [File Layout]
         * 
         * Source: ~/{file_hash}/{wass_version}/{resource_type}/{object_key}
         * i.e. aa6dacf60c0f7afed0713ec8291fcc09c041c6e9b37f1155ab6f4d7977be9c17/v1/{file|tag}/{config|metadata|file}.wass.bin
         * 
         * Local root: "~/files/*", and "~/schemas/{file_hash}/*" to seperate the content, from WASS data.
         * i.e. ~/files/{directory}/{name}.{extension}
         * ~/schemas/aa6dacf60c0f7afed0713ec8291fcc09c041c6e9b37f1155ab6f4d7977be9c17/v1/files/config.json
         * ~/schemas/aa6dacf60c0f7afed0713ec8291fcc09c041c6e9b37f1155ab6f4d7977be9c17/v1/files/metadata.json
         * ~/schemas/aa6dacf60c0f7afed0713ec8291fcc09c041c6e9b37f1155ab6f4d7977be9c17/v1/tags/*
        **/
        private Task<Response<Unit>> Save(string configSourcePath, string metadataSourcePath, string fileSourcePath, JsonFormat config, JsonFormat metadata, byte[] fileData)
        {
            var configPath = FileKey.GetConfigPath(_download.Value.LocalRootPath, configSourcePath);
            var metadataPath = FileKey.GetMetadataPath(_download.Value.LocalRootPath, metadataSourcePath);
            var filePath = FileKey.GetFilePath(_download.Value.LocalRootPath, fileSourcePath);

            var saveConfig = _asset.Save(configPath, config.Value).LogAsync("Config saved to [{File}].".WithArgs(configPath));
            var saveMetadata = _asset.Save(metadataPath, metadata.Value).LogAsync("Metadata saved to [{File}].".WithArgs(metadataPath));
            var saveFile = _asset.Save(filePath, fileData).LogAsync("File saved to [{File}].".WithArgs(filePath));

            return Expression.FunnelAsync(saveConfig, saveMetadata, saveFile, (x, y, z) => Unit.Instance);
        }
    }
}

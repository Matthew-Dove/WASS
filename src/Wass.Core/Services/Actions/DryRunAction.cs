using Amazon.Runtime.Internal;
using ContainerExpressions.Containers;
using Wass.Core.Models;
using Wass.Core.Models.Options;
using Wass.Core.Services.Encryption;
using Wass.Core.Services.Os;

namespace Wass.Core.Services.Actions
{
    public interface IDryRunAction
    {
        Task<Response<Unit>> DryRun(ActionRequest request);
    }

    public sealed class DryRunAction(
        IBackupAction _backup,
        IHash _hash
        ) : IDryRunAction
    {
        public async Task<Response<Unit>> DryRun(ActionRequest request)
        {
            var backupRequest = new ActionRequest
            {
                File = Asset.SandboxFilePath,
                Source = request.Source,
                Compression = SmartEnum<CompressionOptions>.FromObject(CompressionOptions.Brotli),
                Encryption = SmartEnum<EncryptionOptions>.FromObject(EncryptionOptions.Aes),
                IsDryRun = true,
                Tags = Array.Empty<string>()
            };

            var response = await _backup.Backup(backupRequest);
            if (response)
            {
                var fileBytes = Asset.SandboxFileContents.Utf8ToBytes();
                var fileHash = _hash.ComputeHash(fileBytes).BytesToHex();
                Log.Info("DryRun: Changing the file's hash from {RequestFileHash}, to {DryRunFilehash}; so the sandbox file can be found.".WithArgs(request.FileHash, fileHash));
                request.FileHash = fileHash;
            }
            return response;
        }
    }
}

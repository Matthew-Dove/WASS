using ContainerExpressions.Containers;
using Microsoft.Extensions.Options;
using Wass.Core.Models;
using Wass.Core.Models.Configuration;
using Wass.Core.Services.Encryption;
using Wass.Core.Services.Os;

namespace Wass.Core.Services.Actions
{
    public interface IEncryptionAction
    {
        Task<Response<Unit>> EncryptFile(ActionRequest request);
    }

    public sealed class EncryptionAction(
        IAes _aes,
        IAsset _asset,
        IOptions<SecurityConfig> _security
        ) : IEncryptionAction
    {
        public async Task<Response<Unit>> EncryptFile(ActionRequest request)
        {
            var response = new Response<Unit>();
            var outputPath = FileKey.ChangeExtensionEncCmp(request.File, request.Encryption);
            if (_asset.Exists(outputPath).IsTrue(Lambda.Identity)) return response.With(Unit.Instance).Log("The target file {File} already exists.".WithArgs(outputPath));

            var plainbytes = await _asset.Load(request.File).LogAsync("Loaded file [{File}].".WithArgs(request.File));
            var cipherbytes = plainbytes.Bind(x => _aes.Encrypt(_security.Value.Password, x)).Log("Encrypted the file using {Encryption}.".WithArgs(request.Encryption));

            return await cipherbytes.BindAsync(_ => _asset.Save(outputPath, cipherbytes.Value).LogAsync("Saved encrypted file to [{OutputPath}].".WithArgs(outputPath)));
        }
    }
}

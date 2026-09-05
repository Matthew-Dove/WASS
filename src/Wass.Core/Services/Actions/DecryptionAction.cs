using ContainerExpressions.Containers;
using Microsoft.Extensions.Options;
using Wass.Core.Models;
using Wass.Core.Models.Configuration;
using Wass.Core.Services.Encryption;
using Wass.Core.Services.Os;

namespace Wass.Core.Services.Actions
{
    public interface IDecryptionAction
    {
        Task<Response<Unit>> DecryptFile(ActionRequest request);
    }

    public sealed class DecryptionAction(
        IAes _aes,
        IAsset _asset,
        IOptions<SecurityConfig> _security
        ) : IDecryptionAction
    {
        public async Task<Response<Unit>> DecryptFile(ActionRequest request)
        {
            var response = new Response<Unit>();
            var outputPath = FileKey.RevertExtensionEncCmp(request.File, request.Encryption);
            if (_asset.Exists(outputPath).IsTrue(Lambda.Identity)) return response.With(Unit.Instance).Log("The target file {File} already exists.".WithArgs(outputPath));
            
            var cipherbytes = await _asset.Load(request.File).LogAsync("Loaded file [{File}].".WithArgs(request.File));
            var plainbytes = cipherbytes.Bind(x => _aes.Decrypt(_security.Value.Password, x)).Log("Decrypted the file using {Encryption}.".WithArgs(request.Encryption));

            return await plainbytes.BindAsync(_ => _asset.Save(outputPath, plainbytes.Value).LogAsync("Saved decrypted file to [{OutputPath}].".WithArgs(outputPath)));
        }
    }
}

using ContainerExpressions.Containers;
using Wass.Core.Models;
using Wass.Core.Services.Encryption;
using Wass.Core.Services.Os;

namespace Wass.Core.Services.Actions
{
    public interface ISaltAction
    {
        Task<Response<Unit>> CreateSalt(ActionRequest request);
    }

    public sealed class SaltAction(
        IHash _hash,
        IClipboard _clipboard
        ) : ISaltAction
    {
        public Task<Response<Unit>> CreateSalt(ActionRequest request)
        {
            var bytes = _hash.GenerateSalt(request.ByteSize);
            var salt = bytes.BytesToHex();

            if (request.PrintSecret) Console.WriteLine($"SECRET#{salt}".LogValue("Salt sent to stdout."));
            else _clipboard.Set(salt).Log("Salt copied to clipboard.");

            return Task.FromResult(Response.Create(Unit.Instance));
        }
    }
}

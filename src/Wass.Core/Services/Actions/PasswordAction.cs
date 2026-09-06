using ContainerExpressions.Containers;
using Wass.Core.Models;
using Wass.Core.Services.Encryption;
using Wass.Core.Services.Os;

namespace Wass.Core.Services.Actions
{
    public interface IPasswordAction
    {
        Task<Response<Unit>> CreatePassword(ActionRequest request);
    }

    public sealed class PasswordAction(
        IHash _hash,
        IClipboard _clipboard
        ) : IPasswordAction
    {
        public Task<Response<Unit>> CreatePassword(ActionRequest request)
        {
            var password = _hash.GeneratePassword(request.ByteSize);

            if (request.PrintSecret) Console.WriteLine($"SECRET#{password}".LogValue("Password sent to stdout."));
            else _clipboard.Set(password).Log("Password copied to clipboard.");

            return Task.FromResult(Response.Create(Unit.Instance));
        }
    }
}

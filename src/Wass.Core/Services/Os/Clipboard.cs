using ContainerExpressions.Containers;
using TextCopy;

namespace Wass.Core.Services.Os
{
    public interface IClipboard
    {
        Response<string> Get();
        Response<Unit> Set(string text);
        Response<Unit> Clear();
    }

    public sealed class Clipboard : IClipboard
    {
        public Response<string> Get()
        {
            return Try.Run(
                () => ClipboardService.GetText() ?? string.Empty,
                "Error reading text from the clipboard."
            );
        }

        public Response<Unit> Set(string text)
        {
            return Try.Run(
                () => ClipboardService.SetText(text.ThrowIfNullOrEmpty()),
                "Error writing text (length: [{Length}]) to the clipboard.".WithArgs(text?.Length ?? 0)
            ).Transform(Unit.Instance);
        }

        public Response<Unit> Clear()
        {
            return Try.Run(
                () => ClipboardService.SetText(string.Empty),
                "Error attempting to clear the clipboard."
            ).Transform(Unit.Instance);
        }
    }
}
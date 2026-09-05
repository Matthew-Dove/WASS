using ContainerExpressions.Containers;
using Wass.Core.Models;
using Wass.Core.Models.Options;
using Wass.Core.Services.Compression;
using Wass.Core.Services.Os;

namespace Wass.Core.Services.Actions
{
    public interface IDecompressionAction
    {
        Task<Response<Unit>> DecompressFile(ActionRequest request);
    }

    public sealed class DecompressionAction(
        IBrotli _brotli,
        IGZip _gzip,
        IAsset _asset
        ) : IDecompressionAction
    {
        private const int _divisor = 1024;
        private const string _unitDisplay = "KB";

        public async Task<Response<Unit>> DecompressFile(ActionRequest request)
        {
            var outputPath = FileKey.RevertExtensionEncCmp(request.File, request.Compression);
            if (_asset.Exists(outputPath).IsTrue(Lambda.Identity)) return Unit.ResponseSuccess.Log("The target file {File} already exists.".WithArgs(outputPath));

            var data = await _asset.Load(request.File).LogAsync("Loaded file [{File}].".WithArgs(request.File));
            var stream = new Response<byte[]>();

            if (data && request.Compression.HasFlag(CompressionOptions.Brotli)) stream = _brotli.Decompress(data).Log(x => "Brotli decompression size went from {DataSize}{DataUnit} to {DecompressedSize}{CompressedUnit}.".WithArgs(data.Value.Length / _divisor, _unitDisplay, x.Length / _divisor, _unitDisplay));
            if (data && request.Compression.HasFlag(CompressionOptions.GZip)) stream = _gzip.Decompress(data).Log(x => "GZip decompression size went from {DataSize}{DataUnit} to {DecompressedSize}{CompressedUnit}.".WithArgs(data.Value.Length / _divisor, _unitDisplay, x.Length / _divisor, _unitDisplay));

            return await stream.BindAsync(_ => _asset.Save(outputPath, stream.Value).LogAsync("Saved compressed file to [{OutputPath}].".WithArgs(outputPath)));
        }
    }
}

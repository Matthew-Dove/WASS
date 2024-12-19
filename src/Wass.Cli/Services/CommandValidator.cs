using ContainerExpressions.Containers;
using Wass.Cli.Models;
using Wass.Core.Models.Options;
using Wass.Core.Services.Os;

namespace Wass.Cli.Services
{
    public interface ICommandValidator
    {
        bool IsValid(Command command);
    }

    public sealed class CommandValidator(IAsset _asset) : ICommandValidator
    {
        public bool IsValid(Command command)
        {
            var isValid = ValidateFile(command.File).LogValue(x => "{MethodName} OK: {IsValid}.".WithArgs(nameof(ValidateFile), x));

            isValid = isValid && ValidateCompress(command.Options).LogValue(x => "{MethodName} OK: {IsValid}.".WithArgs(nameof(ValidateCompress), x));

            return isValid;
        }

        private bool ValidateFile(string file) => _asset.Exists(file);

        private static bool ValidateCompress(Dictionary<string, string> options)
        {
            if (!(options.ContainsKey(Command.OptionCompress) || options.ContainsKey(Command.OptionCp))) return true;

            var isValid = (!(options.ContainsKey(Command.OptionCompress) & options.ContainsKey(Command.OptionCp)))
                .LogF("Args cannot contain both full, and abbreviated names for the same option: [{FullName)}], and [{AbbreviatedName}]."
                .WithArgs(Command.OptionCompress, Command.OptionCp));

            isValid = isValid && (
                options.TryGetValue(Command.OptionCompress, out var compress) && SmartEnum<CompressionOptions>.FromName(compress) ||
                options.TryGetValue(Command.OptionCp, out var cp) && SmartEnum<CompressionOptions>.FromName(cp)
            ).LogF("Invalid value for the compress option, expected one of: [{CompressionOptions}].".WithArgs(string.Join(", ", SmartEnum<CompressionOptions>.GetNames())));

            return isValid;
        }
    }

    file static class CommandValidatorExtensions
    {
        public static bool LogF(this bool isValid, Format format) { if (!isValid) return isValid.LogValue(format); return isValid; }
    }
}

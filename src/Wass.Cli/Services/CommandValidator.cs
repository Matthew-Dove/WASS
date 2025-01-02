using ContainerExpressions.Containers;
using Microsoft.Extensions.Options;
using Wass.Cli.Models;
using Wass.Core.Models.Configuration;
using Wass.Core.Models.Options;
using Wass.Core.Services.Os;

namespace Wass.Cli.Services
{
    public interface ICommandValidator
    {
        bool IsValid(Command command);
    }

    public sealed class CommandValidator(IAsset _asset, IOptions<DestinationConfig> _config) : ICommandValidator
    {
        public bool IsValid(Command command)
        {
            var isValid = ValidateFile(command.File).LogValue(x => "{MethodName} OK: {IsValid}.".WithArgs(nameof(ValidateFile), x));

            if (command.Verb == Command.VerbBackup)
            {
                isValid = isValid && ValidateDestination(command.Options, _config.Value).LogValue(x => "{MethodName} OK: {IsValid}.".WithArgs(nameof(ValidateDestination), x));
                isValid = isValid && ValidateCompress(command.Options).LogValue(x => "{MethodName} OK: {IsValid}.".WithArgs(nameof(ValidateCompress), x));
                isValid = isValid && ValidateEncrypt(command.Options).LogValue(x => "{MethodName} OK: {IsValid}.".WithArgs(nameof(ValidateEncrypt), x));
            }

            if (command.Verb == Command.VerbRestore)
            {
                isValid = isValid && ValidateDestination(command.Options, _config.Value).LogValue(x => "{MethodName} OK: {IsValid}.".WithArgs(nameof(ValidateDestination), x));
                isValid = isValid && ValidateDecompress(command.Options).LogValue(x => "{MethodName} OK: {IsValid}.".WithArgs(nameof(ValidateDecompress), x));
                isValid = isValid && ValidateDecrypt(command.Options).LogValue(x => "{MethodName} OK: {IsValid}.".WithArgs(nameof(ValidateDecrypt), x));
            }

            return isValid;
        }

        private bool ValidateFile(string file) => _asset.Exists(file);

        private static bool ValidateDestination(Dictionary<string, string> options, DestinationConfig config)
        {
            if (!(options.ContainsKey(Command.OptionDestination) || options.ContainsKey(Command.OptionD))) return false.LogF("S3 compatible destination must be specified.");

            var isValid = (!(options.ContainsKey(Command.OptionDestination) & options.ContainsKey(Command.OptionD)))
                .LogF("Args cannot contain both full, and abbreviated names for the same option: [{FullName)}], and [{AbbreviatedName}]."
                .WithArgs(Command.OptionDestination, Command.OptionD));

            isValid = isValid && (
                config.IsValid()
            ).LogF("Destination config is not valid.");

            isValid = isValid && (
                options.TryGetValue(Command.OptionDestination, out var destination) && config.Sources.TryGetValue(destination, out _) ||
                options.TryGetValue(Command.OptionD, out var d) && config.Sources.TryGetValue(d, out _)
            ).LogF("Invalid value for the destination option, expected one of: [{DestinationOptions}].".WithArgs(string.Join(", ", config.Sources.Keys)));

            return isValid;
        }

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

        private static bool ValidateDecompress(Dictionary<string, string> options)
        {
            if (!(options.ContainsKey(Command.OptionDecompress) || options.ContainsKey(Command.OptionDp))) return true;

            var isValid = (!(options.ContainsKey(Command.OptionCompress) & options.ContainsKey(Command.OptionCp)))
                .LogF("Args cannot contain both full, and abbreviated names for the same option: [{FullName)}], and [{AbbreviatedName}]."
                .WithArgs(Command.OptionDecompress, Command.OptionDp));

            isValid = isValid && (
                options.TryGetValue(Command.OptionDecompress, out var decompress) && SmartEnum<CompressionOptions>.FromName(decompress) ||
                options.TryGetValue(Command.OptionCp, out var dp) && SmartEnum<CompressionOptions>.FromName(dp)
            ).LogF("Invalid value for the decompress option, expected one of: [{CompressionOptions}].".WithArgs(string.Join(", ", SmartEnum<CompressionOptions>.GetNames())));

            return isValid;
        }

        private static bool ValidateEncrypt(Dictionary<string, string> options)
        {
            if (!(options.ContainsKey(Command.OptionEcrypt) || options.ContainsKey(Command.OptionEn))) return true;

            var isValid = (!(options.ContainsKey(Command.OptionEcrypt) & options.ContainsKey(Command.OptionEn)))
                .LogF("Args cannot contain both full, and abbreviated names for the same option: [{FullName)}], and [{AbbreviatedName}]."
                .WithArgs(Command.OptionEcrypt, Command.OptionEn));

            isValid = isValid && (
                options.TryGetValue(Command.OptionEcrypt, out var encrypt) && SmartEnum<EncryptionOptions>.FromName(encrypt) ||
                options.TryGetValue(Command.OptionEn, out var en) && SmartEnum<EncryptionOptions>.FromName(en)
            ).LogF("Invalid value for the encrypt option, expected one of: [{EncryptionOptions}].".WithArgs(string.Join(", ", SmartEnum<EncryptionOptions>.GetNames())));

            return isValid;
        }

        private static bool ValidateDecrypt(Dictionary<string, string> options)
        {
            if (!(options.ContainsKey(Command.OptionDecrypt) || options.ContainsKey(Command.OptionDe))) return true;

            var isValid = (!(options.ContainsKey(Command.OptionDecrypt) & options.ContainsKey(Command.OptionDe)))
                .LogF("Args cannot contain both full, and abbreviated names for the same option: [{FullName)}], and [{AbbreviatedName}]."
                .WithArgs(Command.OptionDecrypt, Command.OptionEn));

            isValid = isValid && (
                options.TryGetValue(Command.OptionDecrypt, out var decrypt) && SmartEnum<EncryptionOptions>.FromName(decrypt) ||
                options.TryGetValue(Command.OptionDe, out var de) && SmartEnum<EncryptionOptions>.FromName(de)
            ).LogF("Invalid value for the decrypt option, expected one of: [{EncryptionOptions}].".WithArgs(string.Join(", ", SmartEnum<EncryptionOptions>.GetNames())));

            return isValid;
        }
    }

    file static class CommandValidatorExtensions
    {
        public static bool LogF(this bool isValid, Format format) { if (!isValid) return isValid.LogValue(format); return isValid; }
    }
}

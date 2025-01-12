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
            var isValid = true;

            if (command.Verb != Command.VerbHelp)
            {
                isValid = ValidateFile(command.File).LogValue(x => "{MethodName} OK: {IsValid}.".WithArgs(nameof(ValidateFile), x));
            }

            if (command.Verb == Command.VerbBackup)
            {
                isValid = isValid && ValidateDestination(command.Options, _config.Value).LogValue(x => "{MethodName} OK: {IsValid}.".WithArgs(nameof(ValidateDestination), x));
                isValid = isValid && ValidateCompress(command.Options, isRequired: false).LogValue(x => "{MethodName} OK: {IsValid}.".WithArgs(nameof(ValidateCompress), x));
                isValid = isValid && ValidateEncrypt(command.Options, isRequired: false).LogValue(x => "{MethodName} OK: {IsValid}.".WithArgs(nameof(ValidateEncrypt), x));
                isValid = isValid && ValidateTags(command.Options, isRequired: false).LogValue(x => "{MethodName} OK: {IsValid}.".WithArgs(nameof(ValidateTags), x));
            }

            if (command.Verb == Command.VerbRestore)
            {
                isValid = isValid && ValidateDestination(command.Options, _config.Value).LogValue(x => "{MethodName} OK: {IsValid}.".WithArgs(nameof(ValidateDestination), x));
                isValid = isValid && ValidateDecompress(command.Options, isRequired: false).LogValue(x => "{MethodName} OK: {IsValid}.".WithArgs(nameof(ValidateDecompress), x));
                isValid = isValid && ValidateDecrypt(command.Options, isRequired: false).LogValue(x => "{MethodName} OK: {IsValid}.".WithArgs(nameof(ValidateDecrypt), x));
            }

            if (command.Verb == Command.VerbTag)
            {
                isValid = isValid && ValidateDestination(command.Options, _config.Value).LogValue(x => "{MethodName} OK: {IsValid}.".WithArgs(nameof(ValidateDestination), x));
                isValid = isValid && ValidateTags(command.Options, isRequired: true).LogValue(x => "{MethodName} OK: {IsValid}.".WithArgs(nameof(ValidateTags), x));
            }

            if (command.Verb == Command.VerbEncryption)
            {
                isValid = isValid && ValidateEncrypt(command.Options, isRequired: true).LogValue(x => "{MethodName} OK: {IsValid}.".WithArgs(nameof(ValidateEncrypt), x));
            }

            if (command.Verb == Command.VerbDecryption)
            {
                isValid = isValid && ValidateDecrypt(command.Options, isRequired: true).LogValue(x => "{MethodName} OK: {IsValid}.".WithArgs(nameof(ValidateDecrypt), x));
            }

            if (command.Verb == Command.VerbCompression)
            {
                isValid = isValid && ValidateCompress(command.Options, isRequired: true).LogValue(x => "{MethodName} OK: {IsValid}.".WithArgs(nameof(ValidateCompress), x));
            }

            if (command.Verb == Command.VerbDecompression)
            {
                isValid = isValid && ValidateDecompress(command.Options, isRequired: true).LogValue(x => "{MethodName} OK: {IsValid}.".WithArgs(nameof(ValidateDecompress), x));
            }

            return isValid;
        }

        private bool ValidateFile(string file) => _asset.Exists(file);

        private static bool ValidateDestination(Dictionary<string, string> options, DestinationConfig config)
        {
            if (!(options.ContainsKey(Command.OptionDestination) || options.ContainsKey(Command.OptionDn))) return false.LogF("S3 compatible destination must be specified.");

            var isValid = (!(options.ContainsKey(Command.OptionDestination) & options.ContainsKey(Command.OptionDn)))
                .LogF("Args cannot contain both full, and abbreviated names for the same option: [{FullName)}], and [{AbbreviatedName}]."
                .WithArgs(Command.OptionDestination, Command.OptionDn));

            isValid = isValid && (
                config.IsValid()
            ).LogF("Destination config is not valid.");

            isValid = isValid && (
                options.TryGetValue(Command.OptionDestination, out var destination) && config.Sources.TryGetValue(destination, out _) ||
                options.TryGetValue(Command.OptionDn, out var d) && config.Sources.TryGetValue(d, out _)
            ).LogF("Invalid value for the destination option, expected one of: [{DestinationOptions}].".WithArgs(string.Join(", ", config.Sources.Keys)));

            return isValid;
        }

        private static bool ValidateCompress(Dictionary<string, string> options, bool isRequired)
        {
            if (!(options.ContainsKey(Command.OptionCompress) || options.ContainsKey(Command.OptionCp))) return (!isRequired).LogF("Compress option must be specified.");

            var isValid = (!(options.ContainsKey(Command.OptionCompress) & options.ContainsKey(Command.OptionCp)))
                .LogF("Args cannot contain both full, and abbreviated names for the same option: [{FullName)}], and [{AbbreviatedName}]."
                .WithArgs(Command.OptionCompress, Command.OptionCp));

            isValid = isValid && (
                options.TryGetValue(Command.OptionCompress, out var compress) && SmartEnum<CompressionOptions>.FromName(compress) ||
                options.TryGetValue(Command.OptionCp, out var cp) && SmartEnum<CompressionOptions>.FromName(cp)
            ).LogF("Invalid value for the compress option, expected one of: [{CompressionOptions}].".WithArgs(string.Join(", ", SmartEnum<CompressionOptions>.GetNames())));

            return isValid;
        }

        private static bool ValidateDecompress(Dictionary<string, string> options, bool isRequired)
        {
            if (!(options.ContainsKey(Command.OptionDecompress) || options.ContainsKey(Command.OptionDp))) return (!isRequired).LogF("Decompress option must be specified.");

            var isValid = (!(options.ContainsKey(Command.OptionCompress) & options.ContainsKey(Command.OptionCp)))
                .LogF("Args cannot contain both full, and abbreviated names for the same option: [{FullName)}], and [{AbbreviatedName}]."
                .WithArgs(Command.OptionDecompress, Command.OptionDp));

            isValid = isValid && (
                options.TryGetValue(Command.OptionDecompress, out var decompress) && SmartEnum<CompressionOptions>.FromName(decompress) ||
                options.TryGetValue(Command.OptionDp, out var dp) && SmartEnum<CompressionOptions>.FromName(dp)
            ).LogF("Invalid value for the decompress option, expected one of: [{CompressionOptions}].".WithArgs(string.Join(", ", SmartEnum<CompressionOptions>.GetNames())));

            return isValid;
        }

        private static bool ValidateEncrypt(Dictionary<string, string> options, bool isRequired)
        {
            if (!(options.ContainsKey(Command.OptionEncrypt) || options.ContainsKey(Command.OptionEn))) return (!isRequired).LogF("Encrypt option must be specified.");

            var isValid = (!(options.ContainsKey(Command.OptionEncrypt) & options.ContainsKey(Command.OptionEn)))
                .LogF("Args cannot contain both full, and abbreviated names for the same option: [{FullName)}], and [{AbbreviatedName}]."
                .WithArgs(Command.OptionEncrypt, Command.OptionEn));

            isValid = isValid && (
                options.TryGetValue(Command.OptionEncrypt, out var encrypt) && SmartEnum<EncryptionOptions>.FromName(encrypt) ||
                options.TryGetValue(Command.OptionEn, out var en) && SmartEnum<EncryptionOptions>.FromName(en)
            ).LogF("Invalid value for the encrypt option, expected one of: [{EncryptionOptions}].".WithArgs(string.Join(", ", SmartEnum<EncryptionOptions>.GetNames())));

            return isValid;
        }

        private static bool ValidateDecrypt(Dictionary<string, string> options, bool isRequired)
        {
            if (!(options.ContainsKey(Command.OptionDecrypt) || options.ContainsKey(Command.OptionDe))) return (!isRequired).LogF("Decrypt option must be specified.");

            var isValid = (!(options.ContainsKey(Command.OptionDecrypt) & options.ContainsKey(Command.OptionDe)))
                .LogF("Args cannot contain both full, and abbreviated names for the same option: [{FullName)}], and [{AbbreviatedName}]."
                .WithArgs(Command.OptionDecrypt, Command.OptionDe));

            isValid = isValid && (
                options.TryGetValue(Command.OptionDecrypt, out var decrypt) && SmartEnum<EncryptionOptions>.FromName(decrypt) ||
                options.TryGetValue(Command.OptionDe, out var de) && SmartEnum<EncryptionOptions>.FromName(de)
            ).LogF("Invalid value for the decrypt option, expected one of: [{EncryptionOptions}].".WithArgs(string.Join(", ", SmartEnum<EncryptionOptions>.GetNames())));

            return isValid;
        }

        private static bool ValidateTags(Dictionary<string, string> options, bool isRequired)
        {
            if (!(options.ContainsKey(Command.OptionTags) || options.ContainsKey(Command.OptionTg))) return (!isRequired).LogF("Tags option must be specified.");

            var isValid = (!(options.ContainsKey(Command.OptionTags) & options.ContainsKey(Command.OptionTg)))
                .LogF("Args cannot contain both full, and abbreviated names for the same option: [{FullName)}], and [{AbbreviatedName}]."
                .WithArgs(Command.OptionTags, Command.OptionTg));

            isValid = isValid && (
                options.TryGetValue(Command.OptionTags, out var tags) && !string.IsNullOrWhiteSpace(tags) ||
                options.TryGetValue(Command.OptionTg, out var tg) && !string.IsNullOrWhiteSpace(tg)
            ).LogF("Invalid value for the tags option, cannot be empty.");

            return isValid;
        }
    }

    file static class CommandValidatorExtensions
    {
        public static bool LogF(this bool isValid, Format format) { if (!isValid) return isValid.LogValue(format); return isValid; }
    }
}

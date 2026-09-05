using ContainerExpressions.Containers;
using Wass.Cli.Models;
using Wass.Core.Models;
using Wass.Core.Models.Options;
using Wass.Core.Services.Actions;

namespace Wass.Cli.Services
{
    public interface ICommandService
    {
        Task<Response<Either<BadRequest, Unit>>> Execute(string[] args);
    }

    public sealed class CommandService(
        ICommandParser _parser,
        ICommandValidator _validator,
        IBackupAction _backup,
        IRestoreAction _restore,
        IEncryptionAction _encryption,
        IDecryptionAction _decryption,
        ICompressionAction _compression,
        IDecompressionAction _decompression
        ) : ICommandService
    {
        public async Task<Response<Either<BadRequest, Unit>>> Execute(string[] args)
        {
            var response = new Response<Either<BadRequest, Unit>>();
            var command = _parser.GetCommand(args);
            var validation = command.Transform(_validator.IsValid);
            if (!validation.IsValid || !validation.Value) return response.With(new BadRequest());

            var verb = command.Value.Verb;
            var request = BuildRequest(command);

            // help
            if (verb == Command.VerbHelp)
            {
                Console.WriteLine(GetHelpText());
                response = response.With(Unit.Instance);
            }

            // backup myfile.txt --destination=s3 --compress=brotli --encrypt=aes
            if (verb == Command.VerbBackup)
            {
                var result = await _backup.Backup(request);
                if (result) response = response.With(Unit.Instance);
            }

            // restore --file-hash=6179a23e... --destination=s3 
            if (verb == Command.VerbRestore)
            {
                var result = await _restore.Restore(request);
                if (result) response = response.With(Unit.Instance);
            }

            if (verb == Command.VerbTag)
            {
                // TODO: Implement tag functionality.
            }

            // encryption myfile.txt --encrypt=aes
            if (verb == Command.VerbEncryption)
            {
                var result = await _encryption.EncryptFile(request);
                if (result) response = response.With(Unit.Instance);
            }

            // decryption myfile.txt.wass.aes --encrypt=aes
            if (verb == Command.VerbDecryption)
            {
                var result = await _decryption.DecryptFile(request);
                if (result) response = response.With(Unit.Instance);
            }

            // compression myfile.txt --compress=brotli
            if (verb == Command.VerbCompression)
            {
                var result = await _compression.CompressFile(request);
                if (result) response = response.With(Unit.Instance);
            }

            // decompression myfile.txt.wass.brotli --compress=brotli
            if (verb == Command.VerbDecompression)
            {
                var result = await _decompression.DecompressFile(request);
                if (result) response = response.With(Unit.Instance);
            }

            if (verb == Command.VerbSalt)
            {
                // TODO: Implement salt functionality.
            }

            if (verb == Command.VerbPassword)
            {
                // TODO: Implement password functionality.
            }

            return response;
        }

        private static ActionRequest BuildRequest(Command command)
        {
            var compression = SmartEnum<CompressionOptions>.FromObject(CompressionOptions.None).Value;
            var compress = command.Options.GetOption(Command.OptionCompress, Command.OptionCp);
            if (compress != string.Empty) compression = SmartEnum<CompressionOptions>.FromName(compress);

            var encryption = SmartEnum<EncryptionOptions>.FromObject(EncryptionOptions.None).Value;
            var encrypt = command.Options.GetOption(Command.OptionEncrypt, Command.OptionEn);
            if (encrypt != string.Empty) encryption = SmartEnum<EncryptionOptions>.FromName(encrypt);

            var isDryRun = command.Flags.HasFlag(Command.FlagDryRun, Command.FlagDr);
            var source = command.Options.GetOption(Command.OptionDestination, Command.OptionDn);
            var tags = GetTags(command.Options);
            var fileHash = command.Options.GetOption(Command.OptionFileHash, Command.OptionFh);
            var useTemplate = !command.Flags.HasFlag(Command.FlagNoTemplate, Command.FlagNt);
            var restoreSchema = !command.Flags.HasFlag(Command.FlagRestoreSchema, Command.FlagNs);

            return new ActionRequest
            {
                IsDryRun = isDryRun,
                Source = source,
                File = command.File,
                Compression = compression,
                Encryption = encryption,
                Tags = tags,
                FileHash = fileHash,
                UseTemplate = useTemplate,
                RestoreSchema = restoreSchema
            };
        }

        private static string[] GetTags(Dictionary<string, string> options)
        {
            var tags = Array.Empty<string>();
            var tag = options.GetOption(Command.OptionTags, Command.OptionTg);

            if (tag != string.Empty)
            {
                var unescapedTags = tag.UnescapeOptionValue();
                tags = Split(unescapedTags.AsSpan(), escape: '\\', delimiter: ':');
            }

            return tags;
        }

        private static string[] Split(ReadOnlySpan<char> input, char escape, char delimiter)
        {
            List<string> segments = new(input.Count(delimiter));
            string slice = string.Empty;
            int start = 0, current = 0;

            while (current < input.Length)
            {
                char c = input[current];

                if (c == escape && current + 1 < input.Length && input[current + 1] == delimiter)
                {
                    current += 2;
                    continue;
                }

                if (c == delimiter)
                {
                    slice = input.Slice(start, current - start).ToString();
                    if (!string.IsNullOrWhiteSpace(slice)) segments.Add(slice);
                    start = current + 1;
                }

                current++;
            }

            slice = input.Slice(start, current - start).ToString();
            if (!string.IsNullOrWhiteSpace(slice)) segments.Add(slice);
            return segments.ToArray();
        }

        // TODO: Update with readme file once commands are done.
        private static string GetHelpText()
        {
            return """

            WASS CLI
            Usage: wass <command> <file> [options]

            Commands:
                backup          Upload the specified file to the configured destination.
                restore         Download the specified file from the configured destination.
                tag             Add tags to the file at a specified destination (colon delimited - "tag1:tag2").
                help            Show help message, and exit.
                encryption      Encrypt a file, and store the result locally.
                decryption      Decrypt a file, and store the result locally.
                compression     Compress a file, and store the result locally.
                decompression   Decompress a file, and store the result locally.
                salt            Generate a cryptographic salt of the specified size in bytes, presented in hex.
                password        Generate a cryptographic password of the specified character length, using a-z, A-Z, 0-9, and special characters.

            Options:
                -cp,   --compress          Compress the file data before backing up: gzip | brotli.
                -en,   --encrypt           Encrypt file data before backing up: aes.
                -dn,   --destination       Specify the backup destination found in the config (API must be S3 compatible).
                -dr,   --dry-run           Simulate the process, with no side effects.
                -nl,   --no-log            Disable logging for the run.
                -tg,   --tags              Add tags to a backed up file.
                -sz,   --size              The size of the salt, or password to generate.
                -fh,   --file-hash         The hash of the file to restore.
                -nt,   --no-template       Will not use a template when creating config files, prevents reusing redundant data.
                -ns,   --no-schema         Won't create WASS metadata objects under the root ~/wass/* directory when restoring a file.

            Examples:
                wass help

                wass backup myfile.txt --destination=s3
                wass backup myfile.txt --destination=s3 --compress=brotli --encrypt=aes --dry-run --no-log

                wass restore myfile.txt --destination=s3

                wass tag myfile.txt --tags="tag1:tag2:tag3"
                wass tag myfile.txt --tags="tag1:tag2:tag3" --encrypt=aes

                wass compression myfile.txt --compress=brotli
                wass decompression myfile.txt.br --compress=brotli

                wass encryption myfile.txt.br --encrypt=aes
                wass decryption myfile.txt.br.bin --encrypt=aes

                wass salt --size=16
                wass password --size=20

            Project:
                https://github.com/matthew-dove/wass

            Exit Codes:
                0: Success.
                1: Error (operation was not successful, or an exception occurred).
                2: Bad Request (invalid cli commands, or arguments).

            """;
        }
    }

    file static class CommandExtensions
    {
        public static string GetOption(this Dictionary<string, string> options, string key, string shortKey)
        {
            _ = options.TryGetValue(key, out var option) || options.TryGetValue(shortKey, out option);
            return option ?? string.Empty;
        }

        public static bool HasFlag(this Dictionary<string, string> flags, string key, string shortKey)
        {
            return flags.ContainsKey(key) || flags.ContainsKey(shortKey);
        }
    }
}

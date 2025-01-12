using ContainerExpressions.Containers;
using Wass.Cli.Models;

namespace Wass.Cli.Services
{
    public interface ICommandService
    {
        Task<Response<Either<BadRequest, Unit>>> Execute(string[] args);
    }

    public sealed class CommandService(ICommandParser _parser, ICommandValidator _validator) : ICommandService
    {
        public async Task<Response<Either<BadRequest, Unit>>> Execute(string[] args)
        {
            var response = new Response<Either<BadRequest, Unit>>();
            var command = _parser.GetCommand(args);
            var validation = command.Transform(_validator.IsValid);
            if (!validation.IsValid || !validation.Value) return response.With(new BadRequest());

            var cmd = command.Value;
            var tags = GetTags(cmd.Options);

            if (cmd.Verb == Command.VerbHelp)
            {
                Console.WriteLine(GetHelpText());
                response = response.With(Unit.Instance);
            }

            // The following functionality should be implemented in the core.

            if (cmd.Verb == Command.VerbBackup)
            {
                // TODO: Implement backup functionality.
            }

            if (cmd.Verb == Command.VerbRestore)
            {
                // TODO: Implement restore functionality.
            }

            if (cmd.Verb == Command.VerbTag)
            {
                // TODO: Implement tag functionality.
            }

            if (cmd.Verb == Command.VerbEncryption)
            {
                // TODO: Implement ecrypt functionality.
            }

            if (cmd.Verb == Command.VerbDecryption)
            {
                // TODO: Implement decrypt functionality.
            }

            if (cmd.Verb == Command.VerbCompression)
            {
                // TODO: Implement compress functionality.
            }

            if (cmd.Verb == Command.VerbDecompression)
            {
                // TODO: Implement decompress functionality.
            }

            await Task.Delay(0);
            return response;
        }

        private static string[] GetTags(Dictionary<string, string> options)
        {
            var tags = Array.Empty<string>();
            var hasTags = options.TryGetValue(Command.OptionTags, out var tg) || options.TryGetValue(Command.OptionTg, out tg);

            if (hasTags)
            {
                var unescapedTags = tg.UnescapeOptionValue();
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

            Options:
                -cp,   --compress          Compress the file data before backing up: gzip | brotli.
                -dp,   --decompress        Decompress the file data before restoring: gzip | brotli.
                -en,   --encrypt           Encrypt file data before backing up: aes.
                -de,   --decrypt           Decrypt file data before restoring: aes.
                -dn,   --destination       Specify the backup destination found in the config (API must be S3 compatible).
                -dr,   --dry-run           Simulate the process, with no side effects.
                -nl,   --no-log            Disable logging for the run.
                -tg,   --tags              Add tags to a backed up file.

            Examples:
                wass backup myfile.txt --destination=s3
                wass backup myfile.txt --destination=s3 --compress=brotli --encrypt=aes --dry-run --no-log

                wass restore myfile.txt --destination=s3 --location="C:\temp\My Files"
                wass restore myfile.txt --destination=s3 --location="C:\temp\My Files" --decompress=brotli --decrypt=aes

                wass tag myfile.txt --tags="tag1:tag2:tag3"
                wass tag myfile.txt --tags="tag1:tag2:tag3" --encrypt=aes

                wass help

                wass encryption myfile.txt --location=./myfile.txt.enc --encrypt=aes
                wass decryption myfile.txt.enc --location=./myfile.txt --decrypt=aes

                wass compression myfile.txt --location=./myfile.txt.zip --compress=gzip
                wass decompression myfile.txt.zip --location=./myfile.txt --decompress=gzip

            Project:
                https://github.com/matthew-dove/wass

            Exit Codes:
                0: Success.
                1: Error (operation was not successful, or an exception occurred).
                2: Bad Request (invalid cli commands, or arguments).

            """;
        }
    }
}

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

            if (cmd.Verb == Command.VerbHelp)
            {
                Console.WriteLine(GetHelpText());
                return response.With(Unit.Instance);
            }

            // Backup, restore, and tag functionality should be implemented in the core.

            var tags = GetTags(cmd.Options);

            if (cmd.Verb == Command.VerbBackup)
            {
                // TODO: Implement backup functionality.
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
                    current += 2; // Skip both the backslash, and colon (\:).
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

            // Get the last segment.
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
                backup      Upload the specified file to the configured destination.
                restore     Download the specified file from the configured destination.
                tag         Add a tag to the file (multiple values separated with a colon ":").
                help        Show help message, and exit.

            Options:
                -cp,   --compress          Compress the file data before backing up: gzip | brotli.
                -dp,   --decompress        Decompress the file data before restoring: gzip | brotli.
                -en,   --encrypt           Encrypt file data before backing up: aes.
                -de,   --decrypt           Decrypt file data before restoring: aes.
                -dn,   --destination       Specify the backup destination found in the config (API must be S3 compatible).
                -dr,   --dry-run           Simulate the process, with no side effects.
                -nl,   --no-log            Disable logging for the run.
                -tg,   --tags              Add tags to a backed up file.

            Project:
                https://github.com/matthew-dove/wass

            Examples:
                wass backup myfile.txt --destination=s3
                wass backup myfile.txt --destination=s3 --compress=brotli --encrypt=aes --dry-run --no-log

                wass restore myfile.txt --destination=s3 --location="C:\temp\My Files"
                wass restore myfile.txt --destination=s3 --location="C:\temp\My Files" --decompress=brotli --decrypt=aes

                wass tag myfile.txt --tags="tag1:tag2:tag3"
                wass tag myfile.txt --tags="tag1:tag2:tag3" --encrypt=aes

                wass help

            """;
        }
    }
}

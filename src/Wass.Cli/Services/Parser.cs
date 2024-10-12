using ContainerExpressions.Containers;
using Wass.Cli.Models;

namespace Wass.Cli.Services
{
    public interface IParser
    {
        Response<Command> GetCommand(string[] args);
    }

    public sealed class Parser : IParser
    {
        public Response<Command> GetCommand(string[] args)
        {
            var response = new Response<Command>();
            if (args.Length == 0) return response.LogErrorValue("No args found.");
            if (args.Length < 2) return response.LogErrorValue("{Args}(s) args found, but expected at least 2 arguments (command verb, and file).".WithArgs(args.Length));
            if (string.IsNullOrWhiteSpace(args[0])) return response.LogErrorValue("Verb cannot be empty: \"{Verb}\".".WithArgs(args[0]));
            if (string.IsNullOrWhiteSpace(args[1])) return response.LogErrorValue("File cannot be empty: \"{File}\".".WithArgs(args[1]));

            var command = new Command { Verb = args[0], File = args[1] };

            // Validate command argument.
            if (!Command.VerbVariants.Contains(command.Verb)) return response.LogErrorValue("Invalid verb: {Verb}, expected one of: {Verbs}.".WithArgs(command.Verb, string.Join(", ", Command.VerbVariants)));

            // Validate file argument (not a comprehensive check, file validation is done futher downstream).
            if (command.File.IndexOfAny(Path.GetInvalidPathChars()) >= 0) return response.LogErrorValue("File has invalid characters: \"{File}\".".WithArgs(command.File));

            // Parse options and flags starting with "--".
            foreach (var arg in args.Skip(2).Where(x => x.StartsWith("--")).Select(x => x.ToLowerInvariant()))
            {
                var parts = arg.Split('=', StringSplitOptions.TrimEntries);
                if (parts.Length < 1 || parts.Length > 2) return response.LogErrorValue("Invalid argument: \"{Argument}\".".WithArgs(arg));
                if (string.IsNullOrWhiteSpace(parts[0]) || (parts.Length == 2 && string.IsNullOrWhiteSpace(parts[1]))) return response.LogErrorValue("Invalid argument: \"{Argument}\".".WithArgs(arg));

                if (parts.Length == 1) // Flag (key only).
                {
                    string flag = parts[0];
                    if (command.Flags.ContainsKey(flag)) return response.LogErrorValue("Duplicated flag: \"{Argument}\".".WithArgs(arg));
                    command.Flags.Add(flag, default);
                }
                else if (parts.Length == 2) // Option (key=value).
                {
                    string key = parts[0], value = parts[1];
                    if (command.Options.ContainsKey(key)) return response.LogErrorValue("Duplicated option: \"{Argument}\".".WithArgs(arg));
                    command.Options.Add(key, value);
                }
            }

            return response.With(command);
        }
    }
}

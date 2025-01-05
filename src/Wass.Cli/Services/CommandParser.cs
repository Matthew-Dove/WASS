using ContainerExpressions.Containers;
using Wass.Cli.Models;

namespace Wass.Cli.Services
{
    public interface ICommandParser
    {
        Response<Command> GetCommand(string[] args);
    }

    /// <summary>
    /// Asserts verbs, options, and flags are formatted correctly, with known commands.
    /// <para>Does not validate the values for options, other than they exist.</para>
    /// <para>Does not validte the file, other than it containing correct characters.</para>
    /// </summary>
    public sealed class CommandParser : ICommandParser
    {
        public Response<Command> GetCommand(string[] args)
        {
            var response = new Response<Command>();
            if (args.Length == 0) return response.LogErrorValue("No args found.");
            if (args.Length == 1) return GetHelpCommand(args[0]);
            if (args.Length < 2) return response.LogErrorValue("{Args}(s) args found, but expected at least 2 arguments (command verb, and file).".WithArgs(args.Length));
            if (string.IsNullOrWhiteSpace(args[0])) return response.LogErrorValue("Verb cannot be empty: \"{Verb}\".".WithArgs(args[0]));
            if (string.IsNullOrWhiteSpace(args[1])) return response.LogErrorValue("File cannot be empty: \"{File}\".".WithArgs(args[1]));

            var command = new Command { Verb = args[0], File = args[1] };

            // Validate the command argument is known.
            if (!Command.VerbVariants.Contains(command.Verb)) return response.LogErrorValue("Invalid verb: {Verb}, expected one of: {Verbs}.".WithArgs(command.Verb, string.Join(", ", Command.VerbVariants)));

            // Validate file argument (not a comprehensive check, file validation is done futher downstream).
            if (command.File.IndexOfAny(Path.GetInvalidPathChars()) >= 0) return response.LogErrorValue("File has invalid characters: \"{File}\".".WithArgs(command.File));

            // Validate the options, and flags are known.
            var options = args.Skip(2);
            foreach (var option in options)
            {
                if (string.IsNullOrWhiteSpace(option)) return response.LogErrorValue("Invalid argument: \"{Argument}\".".WithArgs(option));

                var key = string.Empty;
                if (option.IsOption()) (key, _) = option.SplitOption();

                if (option.IsFlag() && !Command.FlagVariants.Contains(option)) return response.LogErrorValue("Invalid flag: {Flag}, expected one of: {Flags}.".WithArgs(option, string.Join(", ", Command.FlagVariants)));
                if (option.IsOption() && !Command.OptionVariants.Contains(key)) return response.LogErrorValue("Invalid option: {Option}, expected one of: {Options}.".WithArgs(key, string.Join(", ", Command.OptionVariants)));
            }

            // Parse the options and flags.
            foreach (var arg in options.Where(x => x.StartsWith("--") || x.StartsWith("-")))
            {
                if (arg.IsFlag()) // Flag (key only).
                {
                    string flag = arg;
                    if (command.Flags.ContainsKey(flag)) return response.LogErrorValue("Duplicated flag: \"{Argument}\".".WithArgs(arg));
                    command.Flags.Add(flag, default);
                }

                if (arg.IsOption()) // Option (key=value).
                {
                    var (key, value) = arg.SplitOption();
                    if (command.Options.ContainsKey(key)) return response.LogErrorValue("Duplicated option: \"{Argument}\".".WithArgs(arg));
                    command.Options.Add(key, value);
                }
            }

            return response.With(command);
        }

        /// <summary>If there is only one argument, it's expected to be the "help" command.</summary>
        private static Response<Command> GetHelpCommand(string arg)
        {
            var response = new Response<Command>();

            if (
                !Command.VerbHelp.Equals(arg) ||
                !"--help".Equals(arg) ||
                !"-h".Equals(arg)
                )
            {
                return response.LogErrorValue("{Args}(s) args found, but expected at least 2 arguments (command verb, and file).".WithArgs(1));
            }

            return response.With(new Command { Verb = Command.VerbHelp });
        }
    }
}

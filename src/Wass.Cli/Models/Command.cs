namespace Wass.Cli.Models
{
    public sealed class Command
    {
        // Verbs.
        public const string VerbBackup = "backup";
        public const string VerbRestore = "restore";

        // TODO: 
        // These verbs are related to a file's metadata. i.e. date created / modified, folder (restore) path, readonly attribute.
        // public const string VerbTag = "tag"; // Add, or remove a tag on a file (on restore, should tags also be added to the file's details pane?).
        // public const string VerbFilename = "filename"; // Rename a file.
        //
        // These verbs are normally options when backing up, or restoring a file; it might be useful to have them run independently.
        // public const string VerbCompress = "compress";
        // public const string VerbDecompress = "decompress";
        // public const string VerbEncrypt = "encrypt";
        // public const string VerbDecrypt = "decrypt";

        // Options.
        public const string OptionCompress = "--compress", OptionCp = "-cp";
        public const string OptionDecompress = "--decompress", OptionDp = "-dp";
        public const string OptionEcrypt = "--encrypt", OptionEn = "-en";
        public const string OptionDecrypt = "--decrypt", OptionDe = "-de";
        public const string OptionDestination = "--destination", OptionD = "-d";
        public const string OptionTag = "--tag", OptionT = "-t";

        // Flags.
        public const string FlagHelp = "--help", FlagH = "-h";
        public const string FlagDryRun = "--dry-run", FlagDr = "-dr";
        public const string FlagNoLog = "--no-log", FlagNl = "-nl";

        // Variants.
        public static readonly string[] VerbVariants = [VerbBackup, VerbRestore];
        public static readonly string[] OptionVariants = [OptionCompress, OptionCp, OptionDecompress, OptionDp, OptionEcrypt, OptionEn, OptionDecrypt, OptionDe, OptionDestination, OptionD, OptionTag, OptionT];
        public static readonly string[] FlagVariants = [FlagHelp, FlagH, FlagDryRun, FlagDr, FlagNoLog, FlagNl];

        public string Verb { get; set; }
        public string File { get; set; }
        public Dictionary<string, string> Options { get; set; }
        public Dictionary<string, string> Flags { get; set; }

        public Command()
        {
            Verb = string.Empty;
            File = string.Empty;
            Options = new Dictionary<string, string>();
            Flags = new Dictionary<string, string>();
        }
    }

    public static class CommandExtensions
    {
        /// <summary>Flags only have a key.</summary>
        public static bool IsFlag(this string arg) => !IsOption(arg);

        /// <summary>Options have a key, and a value (key=value).</summary>
        public static bool IsOption(this string arg) => arg.Contains("=");

        /// <summary>Breaks the option down into it's key, and value parts.</summary>
        public static (string key, string value) SplitOption(this string arg)
        {
            var index = arg.IndexOf('=');
            return (arg[..index], arg[(index + 1)..]);
        }

        /// <summary>Escapes special delimiters in option values, they are escaped with a backslash (\).</summary>
        public static string EscapeOptionValue(this string value) => value
            .Replace("\\", "\\\\")  // Backslashes (\).
            .Replace(":", "\\:");   // Colons (:).

        /// <summary>Unescapes special delimiters in option values.</summary>
        public static string UnescapeOptionValue(this string value) => value
            .Replace("\\:", ":")    // Colons (:).
            .Replace("\\\\", "\\"); // Backslashes (\).
    }
}

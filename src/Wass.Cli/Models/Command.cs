namespace Wass.Cli.Models
{
    public sealed class Command
    {
        // Verbs.
        public const string VerbBackup = "backup";
        public const string VerbRestore = "restore";
        public const string VerbTag = "tag";
        public const string VerbHelp = "help";
        public const string VerbEncryption = "encryption";
        public const string VerbDecryption = "decryption";
        public const string VerbCompression = "compression";
        public const string VerbDecompression = "decompression";
        public const string VerbSalt = "salt";
        public const string VerbPassword = "password";

        // Options.
        public const string OptionCompress = "--compress", OptionCp = "-cp";
        public const string OptionDecompress = "--decompress", OptionDp = "-dp";
        public const string OptionEncrypt = "--encrypt", OptionEn = "-en";
        public const string OptionDecrypt = "--decrypt", OptionDe = "-de";
        public const string OptionDestination = "--destination", OptionDn = "-dn";
        public const string OptionTags = "--tags", OptionTg = "-tg";
        public const string OptionSize = "--size", OptionSz = "-sz";
        public const string OptionFileHash = "--file-hash", OptionFh = "-fh";

        // Flags.
        public const string FlagDryRun = "--dry-run", FlagDr = "-dr";
        public const string FlagNoLog = "--no-log", FlagNl = "-nl";
        public const string FlagNoTemplate = "--no-template", FlagNt = "-nt";
        public const string FlagRestoreSchema = "--no-schema", FlagNs = "-ns";

        // Variants.
        public static readonly string[] VerbVariants = [VerbBackup, VerbRestore, VerbTag, VerbHelp, VerbEncryption, VerbDecryption, VerbCompression, VerbDecompression, VerbSalt, VerbPassword];
        public static readonly string[] OptionVariants = [OptionCompress, OptionCp, OptionDecompress, OptionDp, OptionEncrypt, OptionEn, OptionDecrypt, OptionDe, OptionDestination, OptionDn, OptionTags, OptionTg, OptionSize, OptionSz, OptionFileHash, OptionFh];
        public static readonly string[] FlagVariants = [FlagDryRun, FlagDr, FlagNoLog, FlagNl, FlagNoTemplate, FlagNt, FlagRestoreSchema, FlagNs];

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

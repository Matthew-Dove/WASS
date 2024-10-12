namespace Wass.Cli.Models
{
    public sealed class Command
    {

        public const string VerbBackup = "backup";
        public const string VerbRestore = "restore";
        public const string VerbCompress = "compress";
        public const string VerbDecompress = "decompress";
        public const string VerbEncrypt = "encrypt";
        public const string VerbDecrypt = "decrypt";
        public const string VerbTag = "tag";

        public const string OptionCompress = "--compress", OptionCp = "cp";
        public const string OptionDecompress = "--decompress", OptionDp = "dp";
        public const string OptionEcrypt = "--encrypt", OptionEn = "en";
        public const string OptionDecrypt = "--decrypt", OptionDe = "de";
        public const string OptionDestination = "--destination", OptionD = "d";
        public const string OptionTag = "--tag", OptionT = "t";

        public const string FlagHelp = "--help", FlagH = "h";
        public const string FlagDryRun = "--dry-run", FlagDr = "dr";
        public const string FlagNoLog = "--no-log", FlagNl = "nl";

        public static readonly string[] VerbVariants = [VerbBackup, VerbRestore, VerbCompress, VerbDecompress, VerbEncrypt, VerbDecrypt, VerbTag];
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
}

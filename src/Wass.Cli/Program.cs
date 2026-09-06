using ContainerExpressions.Containers;
using FrameworkContainers.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Wass.Cli.Models;
using Wass.Cli.Services;
using Wass.Core.Models.Configuration;
using Wass.Core.Models.Options;

namespace Wass.Cli;

internal class Program
{
    private const int _success = 0, _error = 1, _validation = 2;

    static async Task<int> Main(string[] args)
    {
        Try.SetExceptionLogger(Console.Error.WriteLine);
        var code = _error;
        IHost host = null;

        try
        {
            host = BuildHost(ref args);
            var cmd = host.Services.GetRequiredService<ICommandService>();

            var response = await cmd.Execute(args);
            code = response.Transform(static x => x.Match(static _ => _validation, static _ => _success)).GetValueOrDefault(_error);
        }
        catch (Exception ex)
        {
            code = _error;
            ex.LogError("Top level CLI error.");
        }
        finally
        {
            host?.Dispose();
        }

        return code;
    }

    private static IHost BuildHost(ref string[] args)
    {
        var builder = Host.CreateApplicationBuilder();

        /**
         * Config Hierarchy:
         * 1) Command Line Arguments (Highest Priority)
         * 2) Environment Variables
         * 3) appsettings.{stage}.json
         * 4) appsettings.json
        **/
        builder.Configuration.Sources.Clear();

        // Lowest priority config sources are added first.
        builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false); // 4) appsettings.json
        AddStageJsonFile(builder); // 3) appsettings.{stage}.json
        builder.Configuration.AddEnvironmentVariables(); // 2) Environment Variables
        builder.Configuration.AddCommandLine(args); // 1) Command Line Arguments

        // Add any pre-configured args.
        var cliOptions = builder.Configuration[$"{CliConfig.SECTION_NAME}:{nameof(CliConfig.Options)}"];
        args = args.Concat(ParseArgs(cliOptions)).GroupBy(x => x).Select(x => x.First()).ToArray();

        var noLog = args.FirstOrDefault(static x => Command.FlagNl.Equals(x, StringComparison.OrdinalIgnoreCase) || Command.FlagNoLog.Equals(x, StringComparison.OrdinalIgnoreCase)) is not null;
        var isSandbox = args.FirstOrDefault(static x => Command.FlagDr.Equals(x, StringComparison.OrdinalIgnoreCase) || Command.FlagDryRun.Equals(x, StringComparison.OrdinalIgnoreCase)) is not null;
        
        if (noLog) builder.Logging.ClearProviders();

        builder.Services.Configure<SecurityConfig>(builder.Configuration.GetSection(SecurityConfig.SECTION_NAME));
        builder.Services.Configure<DestinationConfig>(builder.Configuration.GetSection(DestinationConfig.SECTION_NAME));
        builder.Services.Configure<DownloadConfig>(builder.Configuration.GetSection(DownloadConfig.SECTION_NAME));

        builder.Services.AddServicesByConvention("Wass.Cli", isSandbox, scanInternals: false, "Wass.", "Wass.Core", "Wass.Infrastructure");

        var host = builder.Build();
        host.Services.AddContainerExpressionsLogging();

        // Clean up config values from the args.
        args = RemoveConfigArgs(builder, args);

        PrimeOptions.ThePump();
        return host;
    }

    private static void AddStageJsonFile(HostApplicationBuilder builder)
    {
        var stage = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
#if DEBUG
        stage = "debug";
#endif
        if (!string.IsNullOrEmpty(stage))
        {
            var path = Path.GetFullPath($"../../../appsettings.{stage}.json");
            if (File.Exists(path))
            {
                builder.Configuration.AddJsonFile(path, optional: true, reloadOnChange: false);
            }
        }
    }

    private static IEnumerable<string> ParseArgs(string commandLine)
    {
        if (string.IsNullOrWhiteSpace(commandLine)) yield break;
        var sb = new System.Text.StringBuilder();
        var inQuotes = false;

        foreach (char c in commandLine)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (char.IsWhiteSpace(c) && !inQuotes)
            {
                if (sb.Length > 0)
                {
                    yield return sb.ToString();
                    sb.Clear();
                }
            }
            else
            {
                sb.Append(c);
            }
        }

        if (sb.Length > 0) yield return sb.ToString();
    }

    private static string[] RemoveConfigArgs(HostApplicationBuilder builder, string[] args)
    {
        var configKeys = builder.Configuration.AsEnumerable()
            .Where(x => x.Key != null && !builder.Configuration.GetSection(x.Key).GetChildren().Any())
            .Select(x => x.Key)
            .Where(x => x.Contains(':'))
            .OrderBy(x => x)
            .ToList();

        var keySet = new HashSet<string>(configKeys, StringComparer.OrdinalIgnoreCase);
        var newArgs = new List<string>(args.Length);

        for (int i = 0; i < args.Length; i++)
        {
            string current = args[i];

            // Case 1: Key=Value format i.e. "--Logging:LogLevel:Default=Debug".
            int eqIndex = current.IndexOf('=');
            if (eqIndex > 0)
            {
                string potentialKey = current[..eqIndex].TrimStart('-', '/');
                if (keySet.TryGetValue(potentialKey, out var originalKey)) continue;
            }

            // Case 2: Space separated format i.e. "Logging:LogLevel:Default Debug".
            string trimmed = current.TrimStart('-', '/');
            if (keySet.TryGetValue(trimmed, out var matchedKey))
            {
                if (i + 1 < args.Length && !args[i + 1].StartsWith('-') && !args[i + 1].StartsWith('/')) i++;
                continue;
            }

            // Keep the arg.
            newArgs.Add(current);
        }

        return newArgs.ToArray();
    }
}

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
            var noLog = args.FirstOrDefault(static x => Command.FlagNl.Equals(x, StringComparison.OrdinalIgnoreCase) || Command.FlagNoLog.Equals(x, StringComparison.OrdinalIgnoreCase)) is not null;
            var isSandbox = args.FirstOrDefault(static x => Command.FlagDr.Equals(x, StringComparison.OrdinalIgnoreCase) || Command.FlagDryRun.Equals(x, StringComparison.OrdinalIgnoreCase)) is not null;

            host = BuildHost(isSandbox, noLog);
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

    private static IHost BuildHost(bool isSandbox, bool noLog)
    {
        var builder = Host.CreateApplicationBuilder();
        if (noLog) builder.Logging.ClearProviders();

#if DEBUG
        isSandbox = true;
        var path = Path.GetFullPath("../../../appsettings.debug.json");
        builder.Configuration.AddJsonFile(path, optional: true, reloadOnChange: false);
#endif

        builder.Services.Configure<SecurityConfig>(builder.Configuration.GetSection(SecurityConfig.SECTION_NAME));
        builder.Services.Configure<DestinationConfig>(builder.Configuration.GetSection(DestinationConfig.SECTION_NAME));
        builder.Services.Configure<DownloadConfig>(builder.Configuration.GetSection(DownloadConfig.SECTION_NAME));

        builder.Services.AddServicesByConvention("Wass.Cli", isSandbox, scanInternals: false, "Wass.", "Wass.Core", "Wass.Infrastructure");

        var host = builder.Build();
        host.Services.AddContainerExpressionsLogging();

        PrimeOptions.ThePump();

        return host;
    }
}

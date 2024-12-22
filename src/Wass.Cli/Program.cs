using ContainerExpressions.Containers;
using FrameworkContainers.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wass.Cli.Models;
using Wass.Cli.Services;
using Wass.Core.Models.Configuration;
using Wass.Core.Models.Options;

namespace Wass.Cli;

internal class Program
{
    private const int _success = 0, _error = 1, _validation = 2;

    /**
     * Exit codes are based on success or failure, common standards are:
     * 0: Success.
     * 1: Error (operation was not successful, or an exception occurred).
     * 2: Bad Request (invalid cli commands, or arguments).
     * 
     * [CLI]
     * 
     * > wass backup {file} [options]
     * > wass restore {file} [options]
     * 
     * > wass compress {file} [options]
     * > wass decompress {file} [options]
     * 
     * > wass encrypt {file} [options]
     * > wass decrypt {file} [options]
     * 
     * > wass tag {file} [options]
     * 
     * > wass backup {file} --compress=brotli --encrypt=aes --tag=video:funny
     * 
     * Dash Style: Use a single dash for short options (-f) and double dash for long options (--file).
     * For parameters that expect a value, use the format --option=value or -o value.
     * > wass backup --file=myfile.txt --destination=s3
     * 
     * It's useful to include a "dry run" option that shows what actions would be taken without making any actual changes.
     * > wass backup myfile.txt --destination=s3 --dry-run
     * 
     * Log errors to stderr, and info messages to stdout, so that they can be handled separately by the caller.
     * If --no-log is specified, information logging is suppressed.
     * > wass backup myfile.txt --destination=s3 --no-log
     * 
     * Include a --help or -h flag for each command to explain it's usage.
     * > wass backup --help
     * Usage: wass backup {file} [options]
     * {file}: Specify the file to backup.
     * 
     * Options:
     *   
     *   -d,    --destination       Specify the backup destination (must be S3 compatible).
     *   -cp,   --compress          Compress the file data before backing up: gzip | brotli.
     *   -en,   --encrypt           Encrypt file data before backing up: aes.
     *   -t,    --tag               Add a tag to the file (multiple values separated with a colon ":").
     *   -h,    --help              Show this help message and exit.
     *   -dr,   --dry-run           Simulate the backup process, with no side effects.
     *   -nl,   --no-log            Disable information logging (errors will still log).
     * 
     * Examples:
     *   
     *   wass backup myfile.txt --destination=s3
     *   wass backup myfile.txt --destination=s3 --compress=brotli --encrypt=aes --tag=joke:funny --dry-run --no-log
     *   
     *   wass restore myfile.txt --destination=s3 --target="C:\temp\My Files"
    **/
    static async Task<int> Main(string[] args)
    {
        Try.SetExceptionLogger(Console.Error.WriteLine);
        var code = _error;
        IHost host = null;

        try
        {
            var isSandbox = args.FirstOrDefault(static x => Command.FlagDr.Equals(x, StringComparison.OrdinalIgnoreCase) || Command.FlagDryRun.Equals(x, StringComparison.OrdinalIgnoreCase)) is not null;
#if DEBUG
            isSandbox = true;
#endif
            host = BuildHost(isSandbox);
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

    private static IHost BuildHost(bool isSandbox)
    {
        var builder = Host.CreateApplicationBuilder();

        builder.Services.Configure<SecurityConfig>(builder.Configuration.GetSection(SecurityConfig.SECTION_NAME));
        builder.Services.Configure<S3Config>(builder.Configuration.GetSection(S3Config.SECTION_NAME));

        builder.Services.AddServicesByConvention("Wass.Cli", isSandbox, "Wass.", "Wass.Core", "Wass.Infrastructure");

        var host = builder.Build();
        host.Services.AddContainerExpressionsLogging();

        PrimeOptions.ThePump();

        return host;
    }
}

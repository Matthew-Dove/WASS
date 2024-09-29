using ContainerExpressions.Containers;
using FrameworkContainers.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Wass.Core.Models.Configuration;
using Wass.Core.Services.Compression;

namespace Wass.Cli;

internal class Program
{
    static async Task Main(string[] args)
    {
        IHost host = null;
        try
        {
            host = BuildHost();

            var zipper = host.Services.GetService<IGZip>();
            var options = host.Services.GetService<IOptions<SecurityConfig>>();

            "wtf??".LogValue(x => x);
            "wtf!!".LogErrorValue(x => x);

            await Task.Delay(0);
        }
        catch (Exception ex)
        {
            ex.LogError("Top level CLI error.");
            if (host is null) throw; // If the builder failed, then logging won't be configured; so throw here instead.
        }
        finally
        {
            host?.Dispose();
        }
    }

    private static IHost BuildHost()
    {
        var builder = Host.CreateApplicationBuilder();

        builder.Services.Configure<SecurityConfig>(builder.Configuration.GetSection(SecurityConfig.SECTION_NAME));
        builder.Services.Configure<S3Config>(builder.Configuration.GetSection(S3Config.SECTION_NAME));

        builder.Services.AddServicesByConvention("Wass.Cli", false, "Wass.", "Wass.Core", "Wass.Infrastructure");

        var host = builder.Build();
        host.Services.AddContainerExpressionsLogging();
        return host;
    }
}

using ContainerExpressions.Containers;
using Microsoft.Extensions.Options;
using Moq;
using Wass.Core.Models.Configuration;
using Wass.Core.Models.Options;

namespace Tests.Wass.Core.Services
{
    public abstract class TestBase
    {
        /// <summary>Replace "S3" with whatever your "Source" is called in the config file under "Destination".</summary>
        protected const string _source = "S3";

        private static readonly DestinationModel _s3 = new DestinationModel
        {
            AccessKeyId = Environment.GetEnvironmentVariable($"Destination__Sources__{_source}__AccessKeyId"),
            SecretAccessKey = Environment.GetEnvironmentVariable($"Destination__Sources__{_source}__SecretAccessKey"),
            Bucket = Environment.GetEnvironmentVariable($"Destination__Sources__{_source}__Bucket"),
            Region = Environment.GetEnvironmentVariable($"Destination__Sources__{_source}__Region") ?? "us-east-2",
            ServiceUrl = Environment.GetEnvironmentVariable($"Destination__Sources__{_source}__ServiceUrl") ?? "https://s3.us-east-2.amazonaws.com/"
        };

        private readonly TestStartUp _startup = new();

        protected IOptions<SecurityConfig> SecurityOptions
        {
            get
            {
                var config = new Mock<IOptions<SecurityConfig>>();
                config.Setup(x => x.Value.Password).Returns("n54AD!:GWUwRrIakgs5npJv:U");
                config.Setup(x => x.Value.Salt).Returns("wvs?pGq01a@@u?5gitFaB.C9c");
                return config.Object;
            }
        }

        protected string Source => _source;

        protected IOptions<DestinationConfig> DestinationConfig
        {
            get
            {
                var config = new DestinationConfig { Sources = new(1) };
                config.Sources.Add(Source, _s3);

                var options = new Mock<IOptions<DestinationConfig>>();
                options.Setup(x => x.Value).Returns(config);

                return options.Object;
            }
        }

        protected IOptions<DownloadConfig> DownloadConfig
        {
            get
            {
                var config = new DownloadConfig { LocalRootPath = @"C:\Wass\Restores\" };

                var options = new Mock<IOptions<DownloadConfig>>();
                options.Setup(x => x.Value).Returns(config);

                return options.Object;
            }
        }
    }

    internal sealed class TestStartUp
    {
        // Set the loggers up only once per application domain.
        static TestStartUp()
        {
            PrimeOptions.ThePump();

            Action<string> trace = x => Console.Out.WriteLine("[Info] {0}", [x]);
            Action<Exception> error = ex => Console.Error.WriteLine("[Error] {0}\r\n{1}", [ex, ex.GetCallerAttributes()]);

#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                trace = x => System.Diagnostics.Debug.WriteLine("[Info] {0}", [x]);
                error = ex => System.Diagnostics.Debug.WriteLine("[Error] {0}\r\n{1}", [ex, ex.GetCallerAttributes()]);
            }
#endif

            Trace.SetLogger(trace);
            Try.SetExceptionLogger(error);
        }
    }
}

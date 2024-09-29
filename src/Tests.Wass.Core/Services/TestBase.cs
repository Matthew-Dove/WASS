using ContainerExpressions.Containers;
using Microsoft.Extensions.Options;
using Moq;
using Wass.Core.Models.Configuration;

namespace Tests.Wass.Core.Services
{
    public abstract class TestBase
    {
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

        protected IOptions<S3Config> S3Options
        {
            get
            {
                var config = new Mock<IOptions<S3Config>>();
                config.Setup(x => x.Value).Returns(new S3Config());
                return config.Object;
            }
        }
    }

    internal sealed class TestStartUp
    {
        // Set the loggers up only once per application domain.
        static TestStartUp()
        {
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

using Ninject.Activation;
using Serilog;

namespace Artemis.Core.Ninject
{
    internal class LoggerProvider : Provider<ILogger>
    {
        private static readonly ILogger _logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .Enrich.WithDemystifiedStackTraces()
            .WriteTo.File("Logs/Artemis log-.txt",
                rollingInterval: RollingInterval.Day,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext:l}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        protected override ILogger CreateInstance(IContext context)
        {
            var requestingType = context.Request.ParentContext?.Plan?.Type;
            if (requestingType != null)
                return _logger.ForContext(requestingType);
            return _logger;
        }
    }
}
using Artemis.Core.DryIoc.Factories;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Artemis.Core.Tests.Infrastructure.Services;

/// <summary>
///     Creates loggers that write to the output of the running test instead of to log files in the Artemis data folder.
/// </summary>
internal sealed class TestLoggerFactory : ILoggerFactory
{
    private readonly ILogger _logger = new LoggerConfiguration()
        .MinimumLevel.Debug()
        .WriteTo.Sink(new TestOutputSink())
        .CreateLogger();

    public ILogger CreateLogger(Type type)
    {
        return _logger.ForContext(type);
    }

    private sealed class TestOutputSink : ILogEventSink
    {
        public void Emit(LogEvent logEvent)
        {
            string source = logEvent.Properties.TryGetValue(Serilog.Core.Constants.SourceContextPropertyName, out LogEventPropertyValue? value) ? value.ToString().Trim('"') : "";
            TestContext.Current.TestOutputHelper?.WriteLine($"[{logEvent.Level}] {source}: {logEvent.RenderMessage()}{(logEvent.Exception != null ? Environment.NewLine + logEvent.Exception : "")}");
        }
    }
}

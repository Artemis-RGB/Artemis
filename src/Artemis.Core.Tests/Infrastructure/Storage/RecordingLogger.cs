using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Artemis.Core.Tests.Infrastructure.Storage;

/// <summary>
///     A logger that keeps everything logged to it, for checking what a class reported
/// </summary>
public sealed class RecordingLogger : ILogEventSink
{
    public RecordingLogger()
    {
        Logger = new LoggerConfiguration().MinimumLevel.Verbose().WriteTo.Sink(this).CreateLogger();
    }

    public ILogger Logger { get; }
    public List<LogEvent> Events { get; } = [];

    public IEnumerable<LogEvent> Errors => Events.Where(e => e.Level == LogEventLevel.Error);

    public void Emit(LogEvent logEvent)
    {
        Events.Add(logEvent);
    }
}

using System;
using System.IO;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Artemis.Core.DryIoc.Factories;

internal class LoggerFactory : ILoggerFactory
{
    internal static readonly LoggingLevelSwitch LoggingLevelSwitch = new(LogEventLevel.Verbose);

    private static readonly ILogger Logger = new LoggerConfiguration()
        .Enrich.FromLogContext()
        .WriteTo.File(Path.Combine(Constants.LogsFolder, "Artemis log-.log"),
            rollingInterval: RollingInterval.Day,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}")
        .WriteTo.Console()
#if DEBUG
        .WriteTo.Debug()
#endif
        .WriteTo.Sink<ArtemisSink>()
        .MinimumLevel.ControlledBy(LoggingLevelSwitch)
        .CreateLogger();

    /// <inheritdoc />
    public ILogger CreateLogger(Type type)
    {
        return Logger.ForContext(type);
    }
}

internal class ArtemisSink : ILogEventSink
{
    /// <inheritdoc />
    public void Emit(LogEvent logEvent)
    {
        LogStore.Emit(logEvent);
    }
}

internal interface ILoggerFactory
{
    ILogger CreateLogger(Type type);
}
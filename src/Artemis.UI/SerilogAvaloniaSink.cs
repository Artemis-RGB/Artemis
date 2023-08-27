using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Avalonia.Logging;
using Serilog;
using AvaloniaLogLevel = Avalonia.Logging.LogEventLevel;
using SerilogLogLevel = Serilog.Events.LogEventLevel;

namespace Artemis.UI;

[SuppressMessage("ReSharper", "TemplateIsNotCompileTimeConstantProblem")]
public class SerilogAvaloniaSink : ILogSink
{
    private readonly ILogger _logger;

    public SerilogAvaloniaSink(ILogger logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public bool IsEnabled(AvaloniaLogLevel level, string area)
    {
        SerilogLogLevel logLevel = GetSerilogLogLevel(level, area);
        
        // Except with binding errors, ignore anything that is information or lower
        return (area == "Binding" || logLevel > SerilogLogLevel.Information) && _logger.IsEnabled(logLevel);
    }

    /// <inheritdoc />
    public void Log(AvaloniaLogLevel level, string area, object? source, string messageTemplate)
    {
        SerilogLogLevel logLevel = GetSerilogLogLevel(level, area);

        ILogger logger = source != null ? _logger.ForContext(source.GetType()) : _logger;
        logger.Write(logLevel, messageTemplate);
    }

    /// <inheritdoc />
    public void Log(AvaloniaLogLevel level, string area, object? source, string messageTemplate, params object?[] propertyValues)
    {
        SerilogLogLevel logLevel = GetSerilogLogLevel(level, area);

        ILogger logger = source != null ? _logger.ForContext(source.GetType()) : _logger;
        logger.Write(logLevel, messageTemplate, propertyValues);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static SerilogLogLevel GetSerilogLogLevel(AvaloniaLogLevel level, string area)
    {
        // Avalonia considers binding errors warnings, we'll treat them Verbose as to not spam people's logs
        // And yes we should fix them instead but we can't always: https://github.com/AvaloniaUI/Avalonia/issues/5762
        if (area == "Binding")
            return SerilogLogLevel.Verbose;
        return (SerilogLogLevel) level;
    }
}
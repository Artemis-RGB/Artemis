using System;
using System.Collections.ObjectModel;
using Artemis.Core;
using Artemis.UI.Shared;
using Avalonia.Threading;
using ReactiveUI;
using Serilog.Events;
using Serilog.Formatting.Display;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using Avalonia.Controls.Documents;
using Avalonia.Media;

namespace Artemis.UI.Screens.Debugger.Logs;

public class LogsDebugViewModel : ActivatableViewModelBase
{
    private readonly MessageTemplateTextFormatter _formatter;

    public InlineCollection Lines { get; } = new InlineCollection();

    private const int MAX_ENTRIES = 1000;

    public LogsDebugViewModel()
    {
        DisplayName = "Logs";

        _formatter = new MessageTemplateTextFormatter(
            "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}"
        );

        foreach (LogEvent logEvent in LogStore.Events)
            AddLogEvent(logEvent);

        this.WhenActivated(disp =>
        {
            LogStore.EventAdded += OnLogEventAdded;

            Disposable.Create(() => { LogStore.EventAdded -= OnLogEventAdded; }).DisposeWith(disp);
        });
    }

    private void OnLogEventAdded(object? sender, LogEventEventArgs e)
    {
        Dispatcher.UIThread.Post(() => { AddLogEvent(e.LogEvent); });
    }

    private void AddLogEvent(LogEvent? logEvent)
    {
        if (logEvent is null)
            return;

        using StringWriter writer = new();
        _formatter.Format(logEvent, writer);
        string line = writer.ToString();

        Lines.Add(new Run(line.TrimEnd('\r', '\n') + '\n')
        {
            Foreground = logEvent.Level switch
            {
                LogEventLevel.Verbose => new SolidColorBrush(Colors.White),
                LogEventLevel.Debug => new SolidColorBrush(Color.FromRgb(216, 216, 216)),
                LogEventLevel.Information => new SolidColorBrush(Color.FromRgb(93, 201, 255)),
                LogEventLevel.Warning => new SolidColorBrush(Color.FromRgb(255, 177, 53)),
                LogEventLevel.Error => new SolidColorBrush(Color.FromRgb(255, 63, 63)),
                LogEventLevel.Fatal => new SolidColorBrush(Colors.Red),
                _ => throw new ArgumentOutOfRangeException()
            }
        });
        LimitLines();
    }

    private void LimitLines()
    {
        if (Lines.Count > MAX_ENTRIES)
            Lines.RemoveRange(0, Lines.Count - MAX_ENTRIES);
    }
}
using Artemis.Core;
using Artemis.UI.Shared;
using Avalonia.Threading;
using AvaloniaEdit.Document;
using Serilog.Events;
using Serilog.Formatting.Display;
using System.IO;

namespace Artemis.UI.Screens.Debugger.Logs;

public class LogsDebugViewModel : ActivatableViewModelBase
{
    private readonly MessageTemplateTextFormatter _formatter;

    public TextDocument Document { get; }

    private const int MAX_ENTRIES = 1000;
    
    public LogsDebugViewModel()
    {
        DisplayName = "Logs";
        Document = new TextDocument();
        _formatter = new MessageTemplateTextFormatter(
            "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}"
        );

        foreach(var logEvent in LogStore.Events)
            AddLogEvent(logEvent);
        
        LogStore.EventAdded += OnLogEventAdded;
    }

    private void OnLogEventAdded(object? sender, LogEventEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            AddLogEvent(e.LogEvent);
        });
    }

    private void AddLogEvent(LogEvent logEvent)
    {
        using StringWriter writer = new();
        _formatter.Format(logEvent, writer);
        Document.Insert(Document.TextLength, writer.ToString());
        while (Document.LineCount > MAX_ENTRIES)
            RemoveOldestLine();
    }

    private void RemoveOldestLine()
    {
        var firstNewLine = Document.Text.IndexOf('\n');
        if (firstNewLine == 0)
            return;
        
        Document.Remove(0, firstNewLine + 1);
    }
}
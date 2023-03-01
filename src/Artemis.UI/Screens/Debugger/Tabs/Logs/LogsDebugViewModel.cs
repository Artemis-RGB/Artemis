using Artemis.Core;
using Artemis.UI.Shared;
using Avalonia.Threading;
using AvaloniaEdit.Document;
using ReactiveUI;
using Serilog.Events;
using Serilog.Formatting.Display;
using System.IO;
using System.Reactive.Disposables;

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

        foreach(LogEvent logEvent in LogStore.Events)
            AddLogEvent(logEvent);
        
        this.WhenActivated(disp =>
        {        
            LogStore.EventAdded += OnLogEventAdded;

            Disposable.Create(() =>
            {
                LogStore.EventAdded -= OnLogEventAdded;
            }).DisposeWith(disp);
        });
    }

    private void OnLogEventAdded(object? sender, LogEventEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            AddLogEvent(e.LogEvent);
        });
    }

    private void AddLogEvent(LogEvent? logEvent)
    {
        if (logEvent is null)
            return;

        using StringWriter writer = new();
        _formatter.Format(logEvent, writer);
        string line = writer.ToString();
        Document.Insert(Document.TextLength, '\n' + line.TrimEnd('\r', '\n'));
        while (Document.LineCount > MAX_ENTRIES)
            RemoveOldestLine();
    }

    private void RemoveOldestLine()
    {
        int firstNewLine = Document.IndexOf('\n', 0, Document.TextLength);
        if (firstNewLine == -1)
        {
            //this should never happen.
            //just in case let's return
            //instead of throwing
            return;
        }      
        
        Document.Remove(0, firstNewLine + 1);
    }
}
using System;
using System.IO;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Routing;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using Avalonia.Threading;
using PropertyChanged.SourceGenerator;
using ReactiveUI;
using Serilog.Events;
using Serilog.Formatting.Display;

namespace Artemis.UI.Screens.Debugger.Routing;

public partial class RoutingDebugViewModel : ActivatableViewModelBase
{
    private readonly IRouter _router;
    private const int MAX_ENTRIES = 1000;
    private readonly MessageTemplateTextFormatter _formatter;
    [Notify] private string? _route;

    public RoutingDebugViewModel(IRouter router)
    {
        _router = router;
        DisplayName = "Routing";
        Reload = ReactiveCommand.CreateFromTask(ExecutReload);
        Navigate = ReactiveCommand.CreateFromTask(ExecuteNavigate, this.WhenAnyValue(vm => vm.Route).Select(r => !string.IsNullOrWhiteSpace(r)));

        _formatter = new MessageTemplateTextFormatter(
            "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}"
        );

        foreach (LogEvent logEvent in LogStore.Events)
            AddLogEvent(logEvent);

        this.WhenActivated(disp =>
        {
            LogStore.EventAdded += OnLogEventAdded;
            Disposable.Create(() => LogStore.EventAdded -= OnLogEventAdded).DisposeWith(disp);
            
            _router.CurrentPath.Subscribe(p => Route = p).DisposeWith(disp);
        });
    }

    public InlineCollection Lines { get; } = new();
    public ReactiveCommand<Unit, Unit> Reload { get; }
    public ReactiveCommand<Unit, Unit> Navigate { get; }

    private void OnLogEventAdded(object? sender, LogEventEventArgs e)
    {
        Dispatcher.UIThread.Post(() => AddLogEvent(e.LogEvent));
    }

    private void AddLogEvent(LogEvent? logEvent)
    {
        if (logEvent is null)
            return;
        if (!logEvent.Properties.TryGetValue("SourceContext", out LogEventPropertyValue? sourceContext) || sourceContext.ToString() != "\"Artemis.UI.Shared.Routing.Navigation\"")
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
    
    private async Task ExecutReload(CancellationToken arg)
    {
        try
        {
            await _router.Reload();
        }
        catch (Exception)
        {
            // ignored
        }
    }

    private async Task ExecuteNavigate(CancellationToken arg)
    {
        try
        {
            if (Route != null)
                await _router.Navigate(Route.Trim());
        }
        catch (Exception)
        {
            // ignored
        }
    }
}
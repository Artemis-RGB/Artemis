using System;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using ReactiveUI;
using Serilog;

namespace Artemis.UI.Screens.Debugger.Logs;

public partial class LogsDebugView : ReactiveUserControl<LogsDebugViewModel>
{
    public LogsDebugView()
    {
        InitializeComponent();
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        Dispatcher.UIThread.Post(() => LogsScrollViewer.ScrollToEnd(), DispatcherPriority.ApplicationIdle);
    }

    private void Control_OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (!(LogsScrollViewer.Extent.Height - LogsScrollViewer.Offset.Y - LogsScrollViewer.Bounds.Bottom <= 60))
            return;
        Dispatcher.UIThread.Post(() => LogsScrollViewer.ScrollToEnd(), DispatcherPriority.Normal);
    }
}
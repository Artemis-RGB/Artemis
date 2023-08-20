using Avalonia.Controls;
using Avalonia.ReactiveUI;
using Avalonia.Threading;

namespace Artemis.UI.Screens.Debugger.Routing;

public partial class RoutingDebugView : ReactiveUserControl<RoutingDebugViewModel>
{
    public RoutingDebugView()
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
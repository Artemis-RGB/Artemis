using System;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Avalonia.Threading;

namespace Artemis.UI.Screens.Debugger.Logs;

public partial class LogsDebugView : ReactiveUserControl<LogsDebugViewModel>
{
    private int _lineCount;

    public LogsDebugView()
    {
        _lineCount = 0;
        InitializeComponent();
    }


    protected override void OnInitialized()
    {
        base.OnInitialized();
        Dispatcher.UIThread.Post(() => LogTextEditor.ScrollToEnd(), DispatcherPriority.ApplicationIdle);
    }

    private void OnTextChanged(object? sender, EventArgs e)
    {
        if (LogTextEditor.ExtentHeight == 0)
            return;

        int linesAdded = LogTextEditor.LineCount - _lineCount;
        double lineHeight = LogTextEditor.ExtentHeight / LogTextEditor.LineCount;
        double outOfScreenTextHeight = LogTextEditor.ExtentHeight - LogTextEditor.VerticalOffset - LogTextEditor.ViewportHeight;
        double outOfScreenLines = outOfScreenTextHeight / lineHeight;

        //we need this help distance because of rounding.
        //if we scroll slightly above the end, we still want it
        //to scroll down to the new lines.
        const double GRACE_DISTANCE = 1d;

        //if we were at the bottom of the log and
        //if the last log event was 5 lines long
        //we will be 5 lines out sync.
        //if this is the case, scroll down.

        //if we are more than that out of sync,
        //the user scrolled up and we should not
        //mess with anything.
        if (_lineCount == 0 || linesAdded + GRACE_DISTANCE >  outOfScreenLines)
        {
            Dispatcher.UIThread.Post(() => LogTextEditor.ScrollToEnd(), DispatcherPriority.ApplicationIdle);
            _lineCount = LogTextEditor.LineCount;
        }
    }
}
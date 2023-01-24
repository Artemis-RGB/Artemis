using System;
using System.Diagnostics;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using AvaloniaEdit;

namespace Artemis.UI.Screens.Debugger.Logs;

public class LogsDebugView : ReactiveUserControl<LogsDebugViewModel>
{
    private int _lineCount;
    private TextEditor _textEditor;

    public LogsDebugView()
    {
        _lineCount = 0;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        _textEditor = this.FindControl<TextEditor>("log");
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _textEditor?.ScrollToEnd();
    }

    private void OnTextChanged(object? sender, EventArgs e)
    {
        if (_textEditor is null)
            return;
        if (_textEditor.ExtentHeight == 0)
            return;

        var linesAdded = _textEditor.LineCount - _lineCount;
        var lineHeight = _textEditor.ExtentHeight / _textEditor.LineCount;
        var outOfScreenTextHeight = _textEditor.ExtentHeight - _textEditor.VerticalOffset - _textEditor.ViewportHeight;
        var outOfScreenLines = outOfScreenTextHeight / lineHeight;

        //we need this help distance because of rounding.
        //if we scroll slightly above the end, we still want it
        //to scroll down to the new lines.
        const double graceDistance = 1d;

        //if we were at the bottom of the log and
        //if the last log event was 5 lines long
        //we will be 5 lines out sync.
        //if this is the case, scroll down.

        //if we are more than that out of sync,
        //the user scrolled up and we should not
        //mess with anything.
        if (_lineCount == 0 || linesAdded + graceDistance >  outOfScreenLines)
        {
            Debug.WriteLine($"Added {linesAdded}, from end {outOfScreenLines}, scrolling");
            _textEditor.ScrollToEnd();
            _lineCount = _textEditor.LineCount;
        }
        else
        {
            Debug.WriteLine($"Added {linesAdded}, from end {outOfScreenLines}, NOT scrolling");
        }
    }
}
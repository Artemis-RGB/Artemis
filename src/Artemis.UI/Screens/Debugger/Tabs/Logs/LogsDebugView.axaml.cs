using System;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using AvaloniaEdit;

namespace Artemis.UI.Screens.Debugger.Logs;

public class LogsDebugView : ReactiveUserControl<LogsDebugViewModel>
{
    public LogsDebugView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void OnTextChanged(object? sender, EventArgs e)
    {
        if (sender is TextEditor te)
            te.ScrollToEnd();
    }
}
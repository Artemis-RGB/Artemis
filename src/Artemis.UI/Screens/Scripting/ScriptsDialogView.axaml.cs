using System.ComponentModel;
using Artemis.UI.Shared;
using Avalonia;
using Avalonia.Markup.Xaml;

namespace Artemis.UI.Screens.Scripting;

public class ScriptsDialogView : ReactiveCoreWindow<ScriptsDialogViewModel>
{
    private bool _canClose;

    public ScriptsDialogView()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
        Closing += OnClosing;
    }

    private async void OnClosing(object? sender, CancelEventArgs e)
    {
        if (_canClose)
            return;

        e.Cancel = true;
        if (ViewModel == null || await ViewModel.CanClose())
        {
            _canClose = true;
            Close();
        }
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
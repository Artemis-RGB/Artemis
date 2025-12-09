using System.ComponentModel;
using Artemis.UI.Shared;
using Avalonia.Markup.Xaml;

namespace Artemis.UI.Screens.Profiles.ProfileEditor.Properties.Windows;

public partial class BrushConfigurationWindowView : ReactiveAppWindow<BrushConfigurationWindowViewModel>
{
    private bool _canClose;

    public BrushConfigurationWindowView()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
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
}
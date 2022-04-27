using System.ComponentModel;
using Avalonia;
using Avalonia.Markup.Xaml;

namespace Artemis.UI.Screens.ProfileEditor.Properties.Windows;

public class EffectConfigurationWindowView : ReactiveCoreWindow<EffectConfigurationWindowViewModel>
{
    public EffectConfigurationWindowView()
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
        if (ViewModel == null)
            return;

        e.Cancel = !await ViewModel.CanClose();
    }
}
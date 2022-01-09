using System.ComponentModel;
using Avalonia;
using Avalonia.Markup.Xaml;

namespace Artemis.UI.Screens.ProfileEditor.ProfileElementProperties.Windows;

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

    private void OnClosing(object? sender, CancelEventArgs e)
    {
        e.Cancel = ViewModel?.CanClose() ?? true;
    }
}
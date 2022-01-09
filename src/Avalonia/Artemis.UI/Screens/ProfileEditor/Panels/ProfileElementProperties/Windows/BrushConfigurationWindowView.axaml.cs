using System.ComponentModel;
using Avalonia;
using Avalonia.Markup.Xaml;

namespace Artemis.UI.Screens.ProfileEditor.ProfileElementProperties.Windows;

public class BrushConfigurationWindowView : ReactiveCoreWindow<EffectConfigurationWindowViewModel>
{
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

    private void OnClosing(object? sender, CancelEventArgs e)
    {
        e.Cancel = ViewModel?.CanClose() ?? true;
    }
}
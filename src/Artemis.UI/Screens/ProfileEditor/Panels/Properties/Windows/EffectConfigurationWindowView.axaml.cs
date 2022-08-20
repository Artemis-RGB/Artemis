using System.ComponentModel;
using Artemis.UI.Shared;
using Avalonia;
using Avalonia.Markup.Xaml;

namespace Artemis.UI.Screens.ProfileEditor.Properties.Windows;

public class EffectConfigurationWindowView : ReactiveCoreWindow<EffectConfigurationWindowViewModel>
{
    private bool _canClose;

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
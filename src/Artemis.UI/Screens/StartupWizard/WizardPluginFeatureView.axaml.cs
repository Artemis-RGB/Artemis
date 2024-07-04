using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Avalonia.Threading;

namespace Artemis.UI.Screens.StartupWizard;

public partial class WizardPluginFeatureView : ReactiveUserControl<WizardPluginFeatureViewModel>
{
    public WizardPluginFeatureView()
    {
        InitializeComponent();
        EnabledToggle.Click += EnabledToggleOnClick;
    }
    
    private void EnabledToggleOnClick(object? sender, RoutedEventArgs e)
    {
        Dispatcher.UIThread.Post(() => ViewModel?.UpdateEnabled(!ViewModel.IsEnabled));
    }
}
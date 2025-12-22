using Avalonia.Interactivity;
using ReactiveUI.Avalonia;
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
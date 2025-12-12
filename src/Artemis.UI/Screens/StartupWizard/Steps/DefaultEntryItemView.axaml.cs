using Avalonia.Input;
using Avalonia.ReactiveUI;

namespace Artemis.UI.Screens.StartupWizard.Steps;

public partial class DefaultEntryItemView : ReactiveUserControl<StartupWizard.Steps.DefaultEntryItemViewModel>
{
    public DefaultEntryItemView()
    {
        InitializeComponent();
    }

    private void HandlePointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (ViewModel != null && !ViewModel.IsInstalled)
        {
            ViewModel.ShouldInstall = !ViewModel.ShouldInstall;
        }
    }
}
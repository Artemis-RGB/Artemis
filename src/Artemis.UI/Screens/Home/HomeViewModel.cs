using Artemis.Core.Services;
using Artemis.UI.Screens.StartupWizard;
using Artemis.UI.Shared.Services;
using Avalonia.Threading;
using ReactiveUI;

namespace Artemis.UI.Screens.Home;

public class HomeViewModel : MainScreenViewModel
{
    public HomeViewModel(IScreen hostScreen, ISettingsService settingsService, IWindowService windowService) : base(hostScreen, "home")
    {
        DisplayName = "Home";

        // Show the startup wizard if it hasn't been completed
        if (!settingsService.GetSetting("UI.SetupWizardCompleted", false).Value)
            Dispatcher.UIThread.InvokeAsync(async () => await windowService.ShowDialogAsync<StartupWizardViewModel, bool>());
    }
}
using Artemis.Core.Services;
using Artemis.UI.Screens.StartupWizard;
using Artemis.UI.Shared.Services;
using Avalonia.Threading;
using ReactiveUI;

namespace Artemis.UI.Screens.Home;

public class HomeViewModel : IMainScreenViewModel
{
    public HomeViewModel(ISettingsService settingsService, IWindowService windowService)
    {
        DisplayName = "Home";

        // Show the startup wizard if it hasn't been completed
        if (!settingsService.GetSetting("UI.SetupWizardCompleted", false).Value)
            Dispatcher.UIThread.InvokeAsync(async () => await windowService.ShowDialogAsync<StartupWizardViewModel, bool>());
    }
}
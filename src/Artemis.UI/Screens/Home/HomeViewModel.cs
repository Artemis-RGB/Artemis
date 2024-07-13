using System.Threading.Tasks;
using Artemis.Core.Services;
using Artemis.UI.Screens.StartupWizard;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Routing;
using Artemis.UI.Shared.Services;
using Avalonia.Threading;

namespace Artemis.UI.Screens.Home;

public class HomeViewModel : RoutableScreen, IMainScreenViewModel
{
    private readonly IRouter _router;

    public HomeViewModel(IRouter router, ISettingsService settingsService, IWindowService windowService)
    {
        _router = router;
        // Show the startup wizard if it hasn't been completed
        if (!settingsService.GetSetting("UI.SetupWizardCompleted", false).Value)
            Dispatcher.UIThread.InvokeAsync(async () => await windowService.ShowDialogAsync<StartupWizardViewModel, bool>());
    }

    public ViewModelBase? TitleBarViewModel => null;
    
    public async Task GetMorePlugins()
    {
        await _router.Navigate("workshop/entries/plugins");
    }
}
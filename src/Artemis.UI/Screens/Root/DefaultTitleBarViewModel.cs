using Artemis.UI.Screens.Workshop.CurrentUser;
using Artemis.UI.Services.Interfaces;
using Artemis.UI.Shared;

namespace Artemis.UI.Screens.Root;

public class DefaultTitleBarViewModel : ViewModelBase
{
    private readonly IDebugService _debugService;

    public DefaultTitleBarViewModel(IDebugService debugService, CurrentUserViewModel currentUserViewModel)
    {
        _debugService = debugService;
        CurrentUserViewModel = currentUserViewModel;
    }

    public CurrentUserViewModel CurrentUserViewModel { get; }

    public void ShowDebugger()
    {
        _debugService.ShowDebugger();
    }
}
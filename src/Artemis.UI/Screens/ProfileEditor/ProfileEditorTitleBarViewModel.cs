using Artemis.UI.Screens.ProfileEditor.MenuBar;
using Artemis.UI.Services.Interfaces;
using Artemis.UI.Shared;

namespace Artemis.UI.Screens.ProfileEditor;

public class ProfileEditorTitleBarViewModel : ViewModelBase
{
    private readonly IDebugService _debugService;

    public ProfileEditorTitleBarViewModel(IDebugService debugService, MenuBarViewModel menuBarViewModel)
    {
        MenuBarViewModel = menuBarViewModel;
        _debugService = debugService;
    }

    public MenuBarViewModel MenuBarViewModel { get; }

    public void ShowDebugger()
    {
        _debugService.ShowDebugger();
    }
}
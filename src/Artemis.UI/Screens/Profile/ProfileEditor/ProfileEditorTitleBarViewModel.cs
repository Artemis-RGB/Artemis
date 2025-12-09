using Artemis.UI.Screens.Profile.ProfileEditor.MenuBar;
using Artemis.UI.Screens.Root;
using Artemis.UI.Shared;

namespace Artemis.UI.Screens.Profile.ProfileEditor;

public class ProfileEditorTitleBarViewModel : ViewModelBase
{
    public ProfileEditorTitleBarViewModel( MenuBarViewModel menuBarViewModel, DefaultTitleBarViewModel defaultTitleBarViewModel)
    {
        MenuBarViewModel = menuBarViewModel;
        DefaultTitleBarViewModel = defaultTitleBarViewModel;
    }

    public MenuBarViewModel MenuBarViewModel { get; }
    public DefaultTitleBarViewModel DefaultTitleBarViewModel { get; }
}
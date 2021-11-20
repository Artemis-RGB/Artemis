using Artemis.UI.Shared;
using ReactiveUI;

namespace Artemis.UI.Screens.Debugger.Tabs.Settings
{
    public class DebugSettingsViewModel : ActivatableViewModelBase, IRoutableViewModel
    {
        public DebugSettingsViewModel(IScreen hostScreen)
        {
            HostScreen = hostScreen;
        }

        public string UrlPathSegment => "logs";
        public IScreen HostScreen { get; }
    }
}
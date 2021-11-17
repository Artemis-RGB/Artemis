using Artemis.UI.Shared;
using ReactiveUI;

namespace Artemis.UI.Screens.Debugger.Tabs.Performance
{
    public class PerformanceDebugViewModel : ActivatableViewModelBase, IRoutableViewModel
    {
        public PerformanceDebugViewModel(IScreen hostScreen)
        {
            HostScreen = hostScreen;
        }

        public string UrlPathSegment => "performance";
        public IScreen HostScreen { get; }
    }
}
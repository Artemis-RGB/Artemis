using ReactiveUI;

namespace Artemis.UI.Avalonia.Screens.Debugger.Tabs.Performance
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
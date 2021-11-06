using ReactiveUI;

namespace Artemis.UI.Avalonia.Screens.Debugger.Tabs.Logs
{
    public class LogsDebugViewModel : ActivatableViewModelBase, IRoutableViewModel
    {
        public LogsDebugViewModel(IScreen hostScreen)
        {
            HostScreen = hostScreen;
        }

        public string UrlPathSegment => "logs";
        public IScreen HostScreen { get; }
    }
}
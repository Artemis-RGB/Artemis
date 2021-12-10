using Artemis.UI.Shared;
using ReactiveUI;

namespace Artemis.UI.Screens.Debugger.Logs
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
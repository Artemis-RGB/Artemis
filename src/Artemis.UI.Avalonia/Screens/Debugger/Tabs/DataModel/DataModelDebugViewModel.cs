using Artemis.UI.Avalonia.Shared;
using ReactiveUI;

namespace Artemis.UI.Avalonia.Screens.Debugger.Tabs.DataModel
{
    public class DataModelDebugViewModel : ActivatableViewModelBase, IRoutableViewModel
    {
        public DataModelDebugViewModel(IScreen hostScreen)
        {
            HostScreen = hostScreen;
        }

        public string UrlPathSegment => "data-model";
        public IScreen HostScreen { get; }
    }
}
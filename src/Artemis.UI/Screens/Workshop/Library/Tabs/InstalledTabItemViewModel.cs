using Artemis.UI.Shared;
using Artemis.WebClient.Workshop.Services;

namespace Artemis.UI.Screens.Workshop.Library.Tabs;

public class InstalledTabItemViewModel : ViewModelBase
{
    public InstalledTabItemViewModel(InstalledEntry installedEntry)
    {
        InstalledEntry = installedEntry;
    }

    public InstalledEntry InstalledEntry { get; }
}
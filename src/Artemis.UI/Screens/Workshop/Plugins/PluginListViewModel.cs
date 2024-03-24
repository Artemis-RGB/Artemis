using Artemis.UI.Screens.Workshop.Entries.List;
using Artemis.UI.Shared.Routing;
using Artemis.WebClient.Workshop;

namespace Artemis.UI.Screens.Workshop.Plugins;

public class PluginListViewModel : RoutableHostScreen<RoutableScreen>
{
    public EntryListViewModel EntryListViewModel { get; }

    public PluginListViewModel(EntryListViewModel entryListViewModel)
    {
        EntryListViewModel = entryListViewModel;
        EntryListViewModel.EntryType = EntryType.Plugin;
    }
}
using Artemis.UI.Screens.Workshop.Entries.List;
using Artemis.UI.Shared.Routing;
using Artemis.WebClient.Workshop;

namespace Artemis.UI.Screens.Workshop.Plugins;

public class PluginListViewModel : RoutableHostScreen<RoutableScreen>
{
    private readonly EntryListViewModel _entryListViewModel;
    public override RoutableScreen DefaultScreen => _entryListViewModel;

    public PluginListViewModel(EntryListViewModel entryListViewModel)
    {
        _entryListViewModel = entryListViewModel;
        _entryListViewModel.EntryType = EntryType.Plugin;
    }
}
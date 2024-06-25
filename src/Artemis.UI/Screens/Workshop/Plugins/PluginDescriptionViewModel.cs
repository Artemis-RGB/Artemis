using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.Screens.Workshop.Entries.List;
using Artemis.UI.Screens.Workshop.EntryReleases;
using Artemis.UI.Shared.Routing;
using Artemis.WebClient.Workshop;
using PropertyChanged.SourceGenerator;

namespace Artemis.UI.Screens.Workshop.Plugins;

public partial class PluginDescriptionViewModel : RoutableScreen
{
    [Notify] private IEntryDetails? _entry;
    [Notify] private List<EntryListItemViewModel>? _dependants;
    private readonly IWorkshopClient _client;
    private readonly Func<IEntrySummary, EntryListItemViewModel> _getEntryListViewModel;

    public PluginDescriptionViewModel(IWorkshopClient client, EntryReleaseInfoViewModel releaseInfoViewModel, Func<IEntrySummary, EntryListItemViewModel> getEntryListViewModel)
    {
        _client = client;
        _getEntryListViewModel = getEntryListViewModel;
        ReleaseInfoViewModel = releaseInfoViewModel;
        ReleaseInfoViewModel.InDetailsScreen = false;
    }

    public EntryReleaseInfoViewModel ReleaseInfoViewModel { get; }

    public async Task SetEntry(IEntryDetails? entry, CancellationToken cancellationToken)
    {
        Entry = entry;

        if (entry != null)
        {
            IReadOnlyList<IEntrySummary>? dependants = (await _client.GetDependantEntries.ExecuteAsync(entry.Id, 0, 25, cancellationToken)).Data?.Entries?.Items;
            Dependants = dependants != null && dependants.Any() ? dependants.Select(_getEntryListViewModel).OrderByDescending(d => d.Entry.Downloads).Take(10).ToList() : null;
        }
        else
        {
            Dependants = null;
        }
    }
}
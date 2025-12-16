using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.Screens.Workshop.Entries.List;
using Artemis.UI.Screens.Workshop.EntryReleases;
using Artemis.UI.Shared.Routing;
using Artemis.WebClient.Workshop;
using PropertyChanged.SourceGenerator;

namespace Artemis.UI.Screens.Workshop.Profile;

public partial class ProfileDescriptionViewModel : RoutableScreen
{
    private readonly IWorkshopClient _client;
    private readonly Func<IEntrySummary, EntryListItemViewModel> _getEntryListViewModel;
    [Notify] private IEntryDetails? _entry;
    [Notify] private List<EntryListItemViewModel>? _dependencies;

    public ProfileDescriptionViewModel(IWorkshopClient client, EntryReleaseInfoViewModel releaseInfoViewModel, Func<IEntrySummary, EntryListItemViewModel> getEntryListViewModel)
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
            IReadOnlyList<IEntrySummary>? dependencies = (await _client.GetLatestDependencies.ExecuteAsync(entry.Id, cancellationToken)).Data?.Entry?.LatestRelease?.Dependencies;
            Dependencies = dependencies != null && dependencies.Any() ? dependencies.Select(_getEntryListViewModel).ToList() : null;
        }
        else
        {
            Dependencies = null;
        }
    }
}
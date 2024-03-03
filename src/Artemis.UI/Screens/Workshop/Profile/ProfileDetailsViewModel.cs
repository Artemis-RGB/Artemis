using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.Screens.Workshop.Entries.Details;
using Artemis.UI.Screens.Workshop.Entries.List;
using Artemis.UI.Screens.Workshop.Parameters;
using Artemis.UI.Shared.Routing;
using Artemis.WebClient.Workshop;
using PropertyChanged.SourceGenerator;
using StrawberryShake;

namespace Artemis.UI.Screens.Workshop.Profile;

public partial class ProfileDetailsViewModel : RoutableScreen<WorkshopDetailParameters>
{
    private readonly IWorkshopClient _client;
    private readonly Func<IEntryDetails, EntryInfoViewModel> _getEntryInfoViewModel;
    private readonly Func<IEntryDetails, EntryReleasesViewModel> _getEntryReleasesViewModel;
    private readonly Func<IEntryDetails, EntryImagesViewModel> _getEntryImagesViewModel;
    private readonly Func<IEntrySummary, EntryListItemViewModel> _getEntryListViewModel;

    [Notify] private IEntryDetails? _entry;
    [Notify] private EntryInfoViewModel? _entryInfoViewModel;
    [Notify] private EntryReleasesViewModel? _entryReleasesViewModel;
    [Notify] private EntryImagesViewModel? _entryImagesViewModel;
    [Notify] private ReadOnlyObservableCollection<EntryListItemViewModel>? _dependencies;

    public ProfileDetailsViewModel(IWorkshopClient client,
        Func<IEntryDetails, EntryInfoViewModel> getEntryInfoViewModel,
        Func<IEntryDetails, EntryReleasesViewModel> getEntryReleasesViewModel,
        Func<IEntryDetails, EntryImagesViewModel> getEntryImagesViewModel,
        Func<IEntrySummary, EntryListItemViewModel> getEntryListViewModel)
    {
        _client = client;
        _getEntryInfoViewModel = getEntryInfoViewModel;
        _getEntryReleasesViewModel = getEntryReleasesViewModel;
        _getEntryImagesViewModel = getEntryImagesViewModel;
        _getEntryListViewModel = getEntryListViewModel;
    }

    public override async Task OnNavigating(WorkshopDetailParameters parameters, NavigationArguments args, CancellationToken cancellationToken)
    {
        await GetEntry(parameters.EntryId, cancellationToken);
    }

    private async Task GetEntry(long entryId, CancellationToken cancellationToken)
    {
        IOperationResult<IGetEntryByIdResult> result = await _client.GetEntryById.ExecuteAsync(entryId, cancellationToken);
        if (result.IsErrorResult())
            return;

        Entry = result.Data?.Entry;
        EntryInfoViewModel = Entry != null ? _getEntryInfoViewModel(Entry) : null;
        EntryReleasesViewModel = Entry != null ? _getEntryReleasesViewModel(Entry) : null;
        EntryImagesViewModel = Entry != null ? _getEntryImagesViewModel(Entry) : null;
        
        IReadOnlyList<IEntrySummary>? dependencies = (await _client.GetLatestDependencies.ExecuteAsync(entryId, cancellationToken)).Data?.Entry?.LatestRelease?.Dependencies;
        Dependencies = dependencies != null && dependencies.Any()
            ? new ReadOnlyObservableCollection<EntryListItemViewModel>(new ObservableCollection<EntryListItemViewModel>(dependencies.Select(_getEntryListViewModel)))
            : null;
    }
}
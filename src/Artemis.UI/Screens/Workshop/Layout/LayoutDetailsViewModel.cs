using System;
using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.Screens.Workshop.Entries.Details;
using Artemis.UI.Screens.Workshop.EntryReleases;
using Artemis.UI.Screens.Workshop.Parameters;
using Artemis.UI.Shared.Routing;
using Artemis.WebClient.Workshop;
using PropertyChanged.SourceGenerator;
using StrawberryShake;

namespace Artemis.UI.Screens.Workshop.Layout;

public partial class LayoutDetailsViewModel : RoutableHostScreen<RoutableScreen, WorkshopDetailParameters>
{
    private readonly IWorkshopClient _client;
    private readonly Func<IEntryDetails, EntryInfoViewModel> _getEntryInfoViewModel;
    private readonly Func<IEntryDetails, EntryReleasesViewModel> _getEntryReleasesViewModel;
    private readonly Func<IEntryDetails, EntryImagesViewModel> _getEntryImagesViewModel;
    [Notify] private IEntryDetails? _entry;
    [Notify] private EntryInfoViewModel? _entryInfoViewModel;
    [Notify] private EntryReleasesViewModel? _entryReleasesViewModel;
    [Notify] private EntryImagesViewModel? _entryImagesViewModel;

    public LayoutDetailsViewModel(IWorkshopClient client,
        LayoutDescriptionViewModel layoutDescriptionViewModel,
        Func<IEntryDetails, EntryInfoViewModel> getEntryInfoViewModel,
        Func<IEntryDetails, EntryReleasesViewModel> getEntryReleasesViewModel,
        Func<IEntryDetails, EntryImagesViewModel> getEntryImagesViewModel)
    {
        _client = client;
        _getEntryInfoViewModel = getEntryInfoViewModel;
        _getEntryReleasesViewModel = getEntryReleasesViewModel;
        _getEntryImagesViewModel = getEntryImagesViewModel;

        LayoutDescriptionViewModel = layoutDescriptionViewModel;
        RecycleScreen = false;
    }

    public LayoutDescriptionViewModel LayoutDescriptionViewModel { get; }

    public override async Task OnNavigating(WorkshopDetailParameters parameters, NavigationArguments args, CancellationToken cancellationToken)
    {
        if (Entry?.Id != parameters.EntryId)
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
        LayoutDescriptionViewModel.Entry = Entry;
    }
}
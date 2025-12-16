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
    private readonly LayoutDescriptionViewModel _layoutDescriptionViewModel;
    private readonly Func<IEntryDetails, EntryReleasesViewModel> _getEntryReleasesViewModel;
    private readonly Func<IEntryDetails, EntryImagesViewModel> _getEntryImagesViewModel;
    [Notify] private IEntryDetails? _entry;
    [Notify] private EntryReleasesViewModel? _entryReleasesViewModel;
    [Notify] private EntryImagesViewModel? _entryImagesViewModel;

    public LayoutDetailsViewModel(IWorkshopClient client,
        LayoutDescriptionViewModel layoutDescriptionViewModel,
        EntryInfoViewModel entryInfoViewModel,
        Func<IEntryDetails, EntryReleasesViewModel> getEntryReleasesViewModel,
        Func<IEntryDetails, EntryImagesViewModel> getEntryImagesViewModel)
    {
        _client = client;
        _layoutDescriptionViewModel = layoutDescriptionViewModel;
        _getEntryReleasesViewModel = getEntryReleasesViewModel;
        _getEntryImagesViewModel = getEntryImagesViewModel;
        
        RecycleScreen = false;
        EntryInfoViewModel = entryInfoViewModel;
    }

    public override RoutableScreen DefaultScreen => _layoutDescriptionViewModel;
    public EntryInfoViewModel EntryInfoViewModel { get; }
    
    public override async Task OnNavigating(WorkshopDetailParameters parameters, NavigationArguments args, CancellationToken cancellationToken)
    {
        if (Entry?.Id != parameters.EntryId)
            await GetEntry(parameters.EntryId, cancellationToken);
    }

    private async Task GetEntry(long entryId, CancellationToken cancellationToken)
    {
        Task grace = Task.Delay(300, cancellationToken);
        IOperationResult<IGetEntryByIdResult> result = await _client.GetEntryById.ExecuteAsync(entryId, cancellationToken);
        if (result.IsErrorResult())
            return;
        
        // Let the UI settle to avoid lag when deep linking
        await grace;

        Entry = result.Data?.Entry;
        EntryInfoViewModel.SetEntry(Entry);
        EntryReleasesViewModel = Entry != null ? _getEntryReleasesViewModel(Entry) : null;
        EntryImagesViewModel = Entry != null ? _getEntryImagesViewModel(Entry) : null;
        _layoutDescriptionViewModel.Entry = Entry;
        _layoutDescriptionViewModel.ReleaseInfoViewModel.Release =  result.Data?.Entry?.LatestRelease;
    }
}
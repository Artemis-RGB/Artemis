using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.Screens.Workshop.Entries.Details;
using Artemis.UI.Screens.Workshop.Entries.List;
using Artemis.UI.Screens.Workshop.EntryReleases;
using Artemis.UI.Screens.Workshop.Parameters;
using Artemis.UI.Shared.Routing;
using Artemis.WebClient.Workshop;
using PropertyChanged.SourceGenerator;
using StrawberryShake;

namespace Artemis.UI.Screens.Workshop.Plugins;

public partial class PluginDetailsViewModel : RoutableHostScreen<RoutableScreen, WorkshopDetailParameters>
{
    private readonly IWorkshopClient _client;
    private readonly PluginDescriptionViewModel _pluginDescriptionViewModel;
    private readonly Func<IEntryDetails, EntryReleasesViewModel> _getEntryReleasesViewModel;
    private readonly Func<IEntryDetails, EntryImagesViewModel> _getEntryImagesViewModel;
    [Notify] private IGetPluginEntryById_Entry? _entry;
    [Notify] private EntryReleasesViewModel? _entryReleasesViewModel;
    [Notify] private EntryImagesViewModel? _entryImagesViewModel;
    [Notify] private ReadOnlyObservableCollection<EntryListItemViewModel>? _dependants;

    public PluginDetailsViewModel(IWorkshopClient client,
        PluginDescriptionViewModel pluginDescriptionViewModel,
        EntryInfoViewModel entryInfoViewModel,
        Func<IEntryDetails, EntryReleasesViewModel> getEntryReleasesViewModel,
        Func<IEntryDetails, EntryImagesViewModel> getEntryImagesViewModel)
    {
        _client = client;
        _pluginDescriptionViewModel = pluginDescriptionViewModel;
        _getEntryReleasesViewModel = getEntryReleasesViewModel;
        _getEntryImagesViewModel = getEntryImagesViewModel;

        EntryInfoViewModel = entryInfoViewModel;
        RecycleScreen = false;
    }

    public override RoutableScreen DefaultScreen => _pluginDescriptionViewModel;
    public EntryInfoViewModel EntryInfoViewModel { get; }

    public override async Task OnNavigating(WorkshopDetailParameters parameters, NavigationArguments args, CancellationToken cancellationToken)
    {
        if (Entry?.Id != parameters.EntryId)
            await GetEntry(parameters.EntryId, cancellationToken);
    }

    private async Task GetEntry(long entryId, CancellationToken cancellationToken)
    {
        Task grace = Task.Delay(300, cancellationToken);
        IOperationResult<IGetPluginEntryByIdResult> result = await _client.GetPluginEntryById.ExecuteAsync(entryId, cancellationToken);
        if (result.IsErrorResult())
            return;

        // Let the UI settle to avoid lag when deep linking
        await grace;
        
        Entry = result.Data?.Entry;
        EntryInfoViewModel.SetEntry(Entry);
        EntryReleasesViewModel = Entry != null ? _getEntryReleasesViewModel(Entry) : null;
        EntryImagesViewModel = Entry != null ? _getEntryImagesViewModel(Entry) : null;

        await _pluginDescriptionViewModel.SetEntry(Entry, cancellationToken);
    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Screens.Workshop.Entries.Details;
using Artemis.UI.Screens.Workshop.Entries.List;
using Artemis.UI.Screens.Workshop.EntryReleases;
using Artemis.UI.Screens.Workshop.Parameters;
using Artemis.UI.Screens.Workshop.Plugins.Dialogs;
using Artemis.UI.Shared.Routing;
using Artemis.UI.Shared.Services;
using Artemis.WebClient.Workshop;
using Artemis.WebClient.Workshop.Models;
using PropertyChanged.SourceGenerator;
using StrawberryShake;

namespace Artemis.UI.Screens.Workshop.Plugins;

public partial class PluginDetailsViewModel : RoutableHostScreen<RoutableScreen, WorkshopDetailParameters>
{
    private readonly IWorkshopClient _client;
    private readonly IWindowService _windowService;
    private readonly IPluginManagementService _pluginManagementService;
    private readonly Func<IEntryDetails, EntryInfoViewModel> _getEntryInfoViewModel;
    private readonly Func<IEntryDetails, EntryReleasesViewModel> _getEntryReleasesViewModel;
    private readonly Func<IEntryDetails, EntryImagesViewModel> _getEntryImagesViewModel;
    [Notify] private IGetPluginEntryById_Entry? _entry;
    [Notify] private EntryInfoViewModel? _entryInfoViewModel;
    [Notify] private EntryReleasesViewModel? _entryReleasesViewModel;
    [Notify] private EntryImagesViewModel? _entryImagesViewModel;
    [Notify] private ReadOnlyObservableCollection<EntryListItemViewModel>? _dependants;

    public PluginDetailsViewModel(IWorkshopClient client,
        IWindowService windowService,
        IPluginManagementService pluginManagementService,
        PluginDescriptionViewModel pluginDescriptionViewModel,
        Func<IEntryDetails, EntryInfoViewModel> getEntryInfoViewModel,
        Func<IEntryDetails, EntryReleasesViewModel> getEntryReleasesViewModel,
        Func<IEntryDetails, EntryImagesViewModel> getEntryImagesViewModel)
    {
        _client = client;
        _windowService = windowService;
        _pluginManagementService = pluginManagementService;
        _getEntryInfoViewModel = getEntryInfoViewModel;
        _getEntryReleasesViewModel = getEntryReleasesViewModel;
        _getEntryImagesViewModel = getEntryImagesViewModel;
        
        PluginDescriptionViewModel = pluginDescriptionViewModel;
        RecycleScreen = false;
    }

    public PluginDescriptionViewModel PluginDescriptionViewModel { get; }
    
    public override async Task OnNavigating(WorkshopDetailParameters parameters, NavigationArguments args, CancellationToken cancellationToken)
    {
        if (Entry?.Id != parameters.EntryId)
            await GetEntry(parameters.EntryId, cancellationToken);
    }

    private async Task GetEntry(long entryId, CancellationToken cancellationToken)
    {
        IOperationResult<IGetPluginEntryByIdResult> result = await _client.GetPluginEntryById.ExecuteAsync(entryId, cancellationToken);
        if (result.IsErrorResult())
            return;

        Entry = result.Data?.Entry;
        EntryInfoViewModel = Entry != null ? _getEntryInfoViewModel(Entry) : null;
        EntryReleasesViewModel = Entry != null ? _getEntryReleasesViewModel(Entry) : null;
        EntryImagesViewModel = Entry != null ? _getEntryImagesViewModel(Entry) : null;

        if (EntryReleasesViewModel != null)
        {
            EntryReleasesViewModel.OnInstallationStarted = OnInstallationStarted;
            EntryReleasesViewModel.OnInstallationFinished = OnInstallationFinished;
        }

        await PluginDescriptionViewModel.SetEntry(Entry, cancellationToken);
    }

    private async Task<bool> OnInstallationStarted(IEntryDetails entryDetails, IRelease release)
    {
        bool confirm = await _windowService.ShowConfirmContentDialog(
            "Installing plugin",
            $"You are about to install version {release.Version} of {entryDetails.Name}. \r\n\r\n" +
            "Plugins are NOT verified by Artemis and could harm your PC, if you have doubts about a plugin please ask on Discord!",
            "I trust this plugin, install it"
        );

        return !confirm;
    }

    private async Task OnInstallationFinished(InstalledEntry installedEntry)
    {
        if (!installedEntry.TryGetMetadata("PluginId", out Guid pluginId))
            return;
        Plugin? plugin = _pluginManagementService.GetAllPlugins().FirstOrDefault(p => p.Guid == pluginId);
        if (plugin == null)
            return;

        await _windowService.CreateContentDialog().WithTitle("Manage plugin").WithViewModel(out PluginDialogViewModel _, plugin).WithFullScreen().ShowAsync();
    }
}
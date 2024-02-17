using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Screens.Workshop.Entries.Details;
using Artemis.UI.Screens.Workshop.Parameters;
using Artemis.UI.Screens.Workshop.Plugins.Dialogs;
using Artemis.UI.Shared.Routing;
using Artemis.UI.Shared.Services;
using Artemis.WebClient.Workshop;
using Artemis.WebClient.Workshop.Models;
using PropertyChanged.SourceGenerator;
using StrawberryShake;

namespace Artemis.UI.Screens.Workshop.Plugins;

public partial class PluginDetailsViewModel : RoutableScreen<WorkshopDetailParameters>
{
    private readonly IWorkshopClient _client;
    private readonly IWindowService _windowService;
    private readonly IPluginManagementService _pluginManagementService;
    private readonly Func<IGetEntryById_Entry, EntryInfoViewModel> _getEntryInfoViewModel;
    private readonly Func<IGetEntryById_Entry, EntryReleasesViewModel> _getEntryReleasesViewModel;
    private readonly Func<IGetEntryById_Entry, EntryImagesViewModel> _getEntryImagesViewModel;
    [Notify] private IGetEntryById_Entry? _entry;
    [Notify] private EntryInfoViewModel? _entryInfoViewModel;
    [Notify] private EntryReleasesViewModel? _entryReleasesViewModel;
    [Notify] private EntryImagesViewModel? _entryImagesViewModel;

    public PluginDetailsViewModel(IWorkshopClient client,
        IWindowService windowService,
        IPluginManagementService pluginManagementService,
        Func<IGetEntryById_Entry, EntryInfoViewModel> getEntryInfoViewModel,
        Func<IGetEntryById_Entry, EntryReleasesViewModel> getEntryReleasesViewModel,
        Func<IGetEntryById_Entry, EntryImagesViewModel> getEntryImagesViewModel)
    {
        _client = client;
        _windowService = windowService;
        _pluginManagementService = pluginManagementService;
        _getEntryInfoViewModel = getEntryInfoViewModel;
        _getEntryReleasesViewModel = getEntryReleasesViewModel;
        _getEntryImagesViewModel = getEntryImagesViewModel;
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
        if (Entry == null)
        {
            EntryInfoViewModel = null;
            EntryReleasesViewModel = null;
        }
        else
        {
            EntryInfoViewModel = _getEntryInfoViewModel(Entry);
            EntryReleasesViewModel = _getEntryReleasesViewModel(Entry);
            EntryImagesViewModel = _getEntryImagesViewModel(Entry);

            EntryReleasesViewModel.OnInstallationStarted = OnInstallationStarted;
            EntryReleasesViewModel.OnInstallationFinished = OnInstallationFinished;
        }
    }

    private async Task<bool> OnInstallationStarted(IEntryDetails entryDetails)
    {
        bool confirm = await _windowService.ShowConfirmContentDialog(
            "Installing plugin",
            $"You are about to install version {entryDetails.LatestRelease?.Version} of {entryDetails.Name}. \r\n\r\n" +
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
using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.DryIoc.Factories;
using Artemis.UI.Extensions;
using Artemis.UI.Screens.Plugins;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Routing;
using Artemis.UI.Shared.Services;
using Artemis.WebClient.Workshop;
using Artemis.WebClient.Workshop.Models;
using Artemis.WebClient.Workshop.Services;
using PropertyChanged.SourceGenerator;
using ReactiveUI;
using StrawberryShake;

namespace Artemis.UI.Screens.Workshop.Library.Tabs;

public partial class InstalledTabItemViewModel : ActivatableViewModelBase
{
    private readonly IWorkshopClient _client;
    private readonly IWorkshopService _workshopService;
    private readonly IRouter _router;
    private readonly IWindowService _windowService;
    private readonly IPluginManagementService _pluginManagementService;
    private readonly ISettingsVmFactory _settingsVmFactory;

    [Notify] private bool _updateAvailable;

    public InstalledTabItemViewModel(InstalledEntry entry,
        IWorkshopClient client,
        IWorkshopService workshopService,
        IRouter router,
        IWindowService windowService,
        IPluginManagementService pluginManagementService,
        ISettingsVmFactory settingsVmFactory)
    {
        _client = client;
        _workshopService = workshopService;
        _router = router;
        _windowService = windowService;
        _pluginManagementService = pluginManagementService;
        _settingsVmFactory = settingsVmFactory;
        Entry = entry;

        this.WhenActivatedAsync(async _ =>
        {
            // Grab the latest entry summary from the workshop
            try
            {
                IOperationResult<IGetEntrySummaryByIdResult> entrySummary = await _client.GetEntrySummaryById.ExecuteAsync(Entry.Id);
                if (entrySummary.Data?.Entry != null)
                {
                    Entry.ApplyEntrySummary(entrySummary.Data.Entry);
                    _workshopService.SaveInstalledEntry(Entry);
                }
            }
            finally
            {
                UpdateAvailable = Entry.ReleaseId != Entry.LatestReleaseId;
            }
        });
    }

    public InstalledEntry Entry { get; }

    public async Task ViewWorkshopPage()
    {
        await _workshopService.NavigateToEntry(Entry.Id, Entry.EntryType);
    }

    public async Task ViewLocal()
    {
        if (Entry.EntryType == EntryType.Profile && Entry.TryGetMetadata("ProfileId", out Guid profileId))
            await _router.Navigate($"profile-editor/{profileId}");
        else if (Entry.EntryType == EntryType.Plugin)
            await _router.Navigate($"workshop/entries/plugins/details/{Entry.Id}/manage");
        else if (Entry.EntryType == EntryType.Layout)
            await _router.Navigate($"workshop/entries/layouts/details/{Entry.Id}/manage");
    }

    public async Task Uninstall()
    {
        bool confirmed = await _windowService.ShowConfirmContentDialog("Do you want to uninstall this entry?", "Both the entry and its contents will be removed.");
        if (!confirmed)
            return;

        // Ideally the installation handler does this but it doesn't have access to the required view models
        if (Entry.EntryType == EntryType.Plugin)
            await UninstallPluginPrerequisites();

        await _workshopService.UninstallEntry(Entry, CancellationToken.None);
    }

    private async Task UninstallPluginPrerequisites()
    {
        if (!Entry.TryGetMetadata("PluginId", out Guid pluginId))
            return;
        Plugin? plugin = _pluginManagementService.GetAllPlugins().FirstOrDefault(p => p.Guid == pluginId);
        if (plugin == null)
            return;

        PluginViewModel pluginViewModel = _settingsVmFactory.PluginViewModel(plugin, ReactiveCommand.Create(() => { }));
        await pluginViewModel.ExecuteRemovePrerequisites(true);
    }
}
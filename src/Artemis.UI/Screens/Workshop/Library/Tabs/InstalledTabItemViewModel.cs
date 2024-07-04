using System;
using System.Linq;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.DryIoc.Factories;
using Artemis.UI.Screens.Plugins;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Routing;
using Artemis.UI.Shared.Services;
using Artemis.WebClient.Workshop;
using Artemis.WebClient.Workshop.Models;
using Artemis.WebClient.Workshop.Services;
using PropertyChanged.SourceGenerator;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.Library.Tabs;

public partial class InstalledTabItemViewModel : ViewModelBase
{
    private readonly IWorkshopService _workshopService;
    private readonly IRouter _router;
    private readonly IWindowService _windowService;
    private readonly IPluginManagementService _pluginManagementService;
    private readonly ISettingsVmFactory _settingsVmFactory;

    public InstalledTabItemViewModel(InstalledEntry installedEntry,
        IWorkshopService workshopService,
        IRouter router, 
        IWindowService windowService,
        IPluginManagementService pluginManagementService,
        ISettingsVmFactory settingsVmFactory)
    {
        _workshopService = workshopService;
        _router = router;
        _windowService = windowService;
        _pluginManagementService = pluginManagementService;
        _settingsVmFactory = settingsVmFactory;
        InstalledEntry = installedEntry;

        ViewWorkshopPage = ReactiveCommand.CreateFromTask(ExecuteViewWorkshopPage);
        ViewLocal = ReactiveCommand.CreateFromTask(ExecuteViewLocal);
        Uninstall = ReactiveCommand.CreateFromTask(ExecuteUninstall);
    }

    public InstalledEntry InstalledEntry { get; }
    public ReactiveCommand<Unit, Unit> ViewWorkshopPage { get; }
    public ReactiveCommand<Unit,Unit> ViewLocal { get; }
    public ReactiveCommand<Unit, Unit> Uninstall { get; }

    private async Task ExecuteViewWorkshopPage()
    {
        await _workshopService.NavigateToEntry(InstalledEntry.EntryId, InstalledEntry.EntryType);
    }

    private async Task ExecuteViewLocal(CancellationToken cancellationToken)
    {
        if (InstalledEntry.EntryType == EntryType.Profile && InstalledEntry.TryGetMetadata("ProfileId", out Guid profileId))
        {
            await _router.Navigate($"profile-editor/{profileId}");
        }
    }
    
    private async Task ExecuteUninstall(CancellationToken cancellationToken)
    {
        bool confirmed = await _windowService.ShowConfirmContentDialog("Do you want to uninstall this entry?", "Both the entry and its contents will be removed.");
        if (!confirmed)
            return;

        // Ideally the installation handler does this but it doesn't have access to the required view models
        if (InstalledEntry.EntryType == EntryType.Plugin)
            await UninstallPluginPrerequisites();
        
        await _workshopService.UninstallEntry(InstalledEntry, cancellationToken);
    }

    private async Task UninstallPluginPrerequisites()
    {
        if (!InstalledEntry.TryGetMetadata("PluginId", out Guid pluginId))
            return;
        Plugin? plugin = _pluginManagementService.GetAllPlugins().FirstOrDefault(p => p.Guid == pluginId);
        if (plugin == null)
            return;

        PluginViewModel pluginViewModel = _settingsVmFactory.PluginViewModel(plugin, ReactiveCommand.Create(() => { }));
        await pluginViewModel.ExecuteRemovePrerequisites(true);
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Threading;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Extensions;
using Artemis.UI.Screens.Workshop.EntryReleases.Dialogs;
using Artemis.UI.Services.Interfaces;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Routing;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.Builders;
using Artemis.UI.Shared.Utilities;
using Artemis.WebClient.Workshop;
using Artemis.WebClient.Workshop.Handlers.InstallationHandlers;
using Artemis.WebClient.Workshop.Models;
using Artemis.WebClient.Workshop.Services;
using PropertyChanged.SourceGenerator;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.EntryReleases;

public partial class EntryReleaseInfoViewModel : ActivatableViewModelBase
{
    private readonly IRouter _router;
    private readonly INotificationService _notificationService;
    private readonly IWindowService _windowService;
    private readonly IWorkshopService _workshopService;
    private readonly IPluginManagementService _pluginManagementService;
    private readonly IPluginInteractionService _pluginInteractionService;
    private readonly Progress<StreamProgress> _progress = new();

    [Notify] private IReleaseDetails? _release;
    [Notify] private float _installProgress;
    [Notify] private bool _isCurrentVersion;
    [Notify] private bool _installationInProgress;
    [Notify] private bool _inDetailsScreen;
    [Notify] private string? _incompatibilityReason;

    private CancellationTokenSource? _cts;

    public EntryReleaseInfoViewModel(IRouter router,
        INotificationService notificationService,
        IWindowService windowService,
        IWorkshopService workshopService,
        IPluginManagementService pluginManagementService,
        IPluginInteractionService pluginInteractionService)
    {
        _router = router;
        _notificationService = notificationService;
        _windowService = windowService;
        _workshopService = workshopService;
        _pluginManagementService = pluginManagementService;
        _pluginInteractionService = pluginInteractionService;
        _progress.ProgressChanged += (_, f) => InstallProgress = f.ProgressPercentage;

        this.WhenActivated(d =>
        {
            _workshopService.OnEntryInstalled += WorkshopServiceOnOnEntryInstalled;
            _workshopService.OnEntryUninstalled += WorkshopServiceOnOnEntryInstalled;
            Disposable.Create(() =>
            {
                _workshopService.OnEntryInstalled -= WorkshopServiceOnOnEntryInstalled;
                _workshopService.OnEntryUninstalled -= WorkshopServiceOnOnEntryInstalled;
            }).DisposeWith(d);

            IsCurrentVersion = Release != null && _workshopService.GetInstalledEntry(Release.Entry.Id)?.ReleaseId == Release.Id;
            IncompatibilityReason = Release != null && !Release.IsCompatible() ? $"Requires Artemis v{Release.MinimumVersion} or later" : null;
        });

        this.WhenAnyValue(vm => vm.Release).Subscribe(r => IsCurrentVersion = r != null && _workshopService.GetInstalledEntry(r.Entry.Id)?.ReleaseId == r.Id);
        this.WhenAnyValue(vm => vm.Release).Subscribe(r => IncompatibilityReason = r != null && !r.IsCompatible() ? $"Requires Artemis v{r.MinimumVersion} or later" : null);

        InDetailsScreen = true;
    }

    private void WorkshopServiceOnOnEntryInstalled(object? sender, InstalledEntry e)
    {
        IsCurrentVersion = Release != null && _workshopService.GetInstalledEntry(Release.Entry.Id)?.ReleaseId == Release.Id;
    }

    public async Task Close()
    {
        await _router.GoUp();
    }

    public async Task Install()
    {
        if (Release == null)
            return;

        // If the entry has missing dependencies, show a dialog
        List<IEntrySummary> missing = Release.Dependencies.Where(d => _workshopService.GetInstalledEntry(d.Id) == null).Cast<IEntrySummary>().ToList();
        if (missing.Count > 0)
        {
            await _windowService.CreateContentDialog()
                .WithTitle("Requirements missing")
                .WithViewModel(out DependenciesDialogViewModel _, Release.Entry, missing)
                .WithCloseButtonText("Cancel installation")
                .ShowAsync();
            return;
        }

        // If not the latest version, warn and offer to disable auto-updates
        bool disableAutoUpdates = false;
        if (Release.Id != Release.Entry.LatestReleaseId)
        {
            disableAutoUpdates = await _windowService.ShowConfirmContentDialog(
                "You are installing an older version of this entry",
                "Would you like to disable auto-updates for this entry?",
                "Yes",
                "No"
            );
        }

        _cts = new CancellationTokenSource();
        InstallProgress = 0;
        InstallationInProgress = true;
        try
        {
            EntryInstallResult result = await _workshopService.InstallEntry(Release.Entry, Release, _progress, _cts.Token);
            if (result.IsSuccess && result.Entry != null)
            {
                _workshopService.SetAutoUpdate(result.Entry, !disableAutoUpdates);
                _notificationService.CreateNotification().WithTitle("Installation succeeded").WithSeverity(NotificationSeverity.Success).Show();
                InstallationInProgress = false;

                // Auto-enable plugins as the installation handler won't deal with the required UI interactions
                if (result.Installed is Plugin installedPlugin)
                    await AutoEnablePlugin(installedPlugin);
            }
            else if (!_cts.IsCancellationRequested)
            {
                if (result.Exception != null)
                {
                    // Not taking the fall on this one :')
                    _windowService.ShowExceptionDialog(
                        "Failed to install workshop entry",
                        result.Exception,
                        "Make sure the entry is compatible with this version of Artemis or reach out to the author for support."
                    );
                }
                else
                    await _windowService.ShowConfirmContentDialog("Failed to install workshop entry", result.Message ?? "Unknown error", "Close", null);
            }
        }
        catch (Exception e)
        {
            _windowService.ShowExceptionDialog("Failed to install workshop entry", e);
        }
        finally
        {
            InstallationInProgress = false;
        }
    }

    public async Task Reinstall()
    {
        if (await _windowService.ShowConfirmContentDialog("Reinstall entry", "Are you sure you want to reinstall this entry?"))
            await Install();
    }

    public async Task Uninstall()
    {
        InstalledEntry? installedEntry = _workshopService.GetInstalledEntry(Release!.Entry.Id);
        if (installedEntry == null)
            return;

        InstallationInProgress = true;
        try
        {
            bool confirmed = await _windowService.ShowConfirmContentDialog("Do you want to uninstall this entry?", "Both the entry and its contents will be removed.");
            if (!confirmed)
                return;

            // Ideally the installation handler does this but it doesn't have access to the required view models
            if (installedEntry.EntryType == EntryType.Plugin)
                await UninstallPluginPrerequisites(installedEntry);

            await _workshopService.UninstallEntry(installedEntry, CancellationToken.None);

            _notificationService.CreateNotification().WithTitle("Entry uninstalled").WithSeverity(NotificationSeverity.Success).Show();
        }
        finally
        {
            InstallationInProgress = false;
        }
    }

    public void Cancel()
    {
        _cts?.Cancel();
    }

    private async Task UninstallPluginPrerequisites(InstalledEntry installedEntry)
    {
        if (!installedEntry.TryGetMetadata("PluginId", out Guid pluginId))
            return;
        Plugin? plugin = _pluginManagementService.GetAllPlugins().FirstOrDefault(p => p.Guid == pluginId);
        if (plugin == null)
            return;

        await _pluginInteractionService.RemovePluginPrerequisites(plugin, true);
    }

    private async Task AutoEnablePlugin(Plugin plugin)
    {
        // There's quite a bit of UI involved in enabling a plugin, borrowing the PluginSettingsViewModel for this
        await _pluginInteractionService.EnablePlugin(plugin, true);

        // Find features without prerequisites to enable
        foreach (PluginFeatureInfo pluginFeatureInfo in plugin.Features)
        {
            if (pluginFeatureInfo.Instance == null || pluginFeatureInfo.Instance.IsEnabled || pluginFeatureInfo.Prerequisites.Count != 0)
                continue;

            try
            {
                _pluginManagementService.EnablePluginFeature(pluginFeatureInfo.Instance, true);
            }
            catch (Exception e)
            {
                _notificationService.CreateNotification().WithTitle("Failed to enable plugin feature").WithMessage(e.Message).WithSeverity(NotificationSeverity.Error).Show();
            }
        }
    }
}
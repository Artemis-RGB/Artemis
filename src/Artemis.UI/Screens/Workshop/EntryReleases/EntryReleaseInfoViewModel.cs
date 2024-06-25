using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.DryIoc.Factories;
using Artemis.UI.Screens.Plugins;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Routing;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.Builders;
using Artemis.UI.Shared.Utilities;
using Artemis.WebClient.Workshop;
using Artemis.WebClient.Workshop.Handlers.InstallationHandlers;
using Artemis.WebClient.Workshop.Models;
using Artemis.WebClient.Workshop.Services;
using Humanizer;
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
    private readonly EntryInstallationHandlerFactory _factory;
    private readonly ISettingsVmFactory _settingsVmFactory;
    private readonly Progress<StreamProgress> _progress = new();
    private readonly ObservableAsPropertyHelper<bool> _isCurrentVersion;

    [Notify] private IReleaseDetails? _release;
    [Notify] private float _installProgress;
    [Notify] private bool _installationInProgress;
    [Notify] private bool _inDetailsScreen;

    private CancellationTokenSource? _cts;

    public EntryReleaseInfoViewModel(IRouter router,
        INotificationService notificationService,
        IWindowService windowService,
        IWorkshopService workshopService,
        IPluginManagementService pluginManagementService,
        EntryInstallationHandlerFactory factory,
        ISettingsVmFactory settingsVmFactory)
    {
        _router = router;
        _notificationService = notificationService;
        _windowService = windowService;
        _workshopService = workshopService;
        _pluginManagementService = pluginManagementService;
        _factory = factory;
        _settingsVmFactory = settingsVmFactory;
        _progress.ProgressChanged += (_, f) => InstallProgress = f.ProgressPercentage;

        _isCurrentVersion = this.WhenAnyValue(vm => vm.Release, vm => vm.InstallationInProgress, (release, _) => release)
            .Select(r => r != null && _workshopService.GetInstalledEntry(r.Entry.Id)?.ReleaseId == r.Id)
            .ToProperty(this, vm => vm.IsCurrentVersion);

        InDetailsScreen = true;
    }

    public bool IsCurrentVersion => _isCurrentVersion.Value;

    public async Task Close()
    {
        await _router.GoUp();
    }

    public async Task Install()
    {
        if (Release == null)
            return;

        // If the entry has missing dependencies, show a dialog
        foreach (IGetEntryById_Entry_LatestRelease_Dependencies dependency in Release.Dependencies)
        {
            if (_workshopService.GetInstalledEntry(dependency.Id) == null)
            {
                if (await _windowService.ShowConfirmContentDialog("Missing dependencies",
                        $"One or more dependencies are missing, this {Release.Entry.EntryType.Humanize(LetterCasing.LowerCase)} won't work without them", "View dependencies"))
                    await _router.GoUp();
                return;
            }
        }

        _cts = new CancellationTokenSource();
        InstallProgress = 0;
        InstallationInProgress = true;
        try
        {
            IEntryInstallationHandler handler = _factory.CreateHandler(Release.Entry.EntryType);
            EntryInstallResult result = await handler.InstallAsync(Release.Entry, Release, _progress, _cts.Token);
            if (result.IsSuccess)
            {
                _notificationService.CreateNotification().WithTitle("Installation succeeded").WithSeverity(NotificationSeverity.Success).Show();
                InstallationInProgress = false;
                await Manage();
            }
            else if (!_cts.IsCancellationRequested)
                _notificationService.CreateNotification().WithTitle("Installation failed").WithMessage(result.Message).WithSeverity(NotificationSeverity.Error).Show();
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

    public async Task Manage()
    {
        if (Release?.Entry.EntryType != EntryType.Profile)
            await _router.Navigate("../../manage", new RouterNavigationOptions {AdditionalArguments = true});
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

            IEntryInstallationHandler handler = _factory.CreateHandler(installedEntry.EntryType);
            await handler.UninstallAsync(installedEntry, CancellationToken.None);
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

        PluginViewModel pluginViewModel = _settingsVmFactory.PluginViewModel(plugin, ReactiveCommand.Create(() => { }));
        await pluginViewModel.ExecuteRemovePrerequisites(true);
    }
}
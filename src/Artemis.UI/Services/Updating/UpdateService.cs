using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.Storage.Entities.General;
using Artemis.Storage.Repositories.Interfaces;
using Artemis.UI.Screens.Settings.Updating;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.MainWindow;
using Artemis.WebClient.Updating;
using Avalonia.Threading;
using Serilog;
using StrawberryShake;
using Timer = System.Timers.Timer;

namespace Artemis.UI.Services.Updating;

public class UpdateService : IUpdateService
{
    private const double UPDATE_CHECK_INTERVAL = 3_600_000; // once per hour
    private readonly PluginSetting<bool> _autoCheck;
    private readonly PluginSetting<bool> _autoInstall;
    private readonly PluginSetting<string> _channel;
    private readonly Func<string, ReleaseInstaller> _getReleaseInstaller;

    private readonly ILogger _logger;
    private readonly IMainWindowService _mainWindowService;
    private readonly IQueuedActionRepository _queuedActionRepository;
    private readonly Lazy<IUpdateNotificationProvider> _updateNotificationProvider;
    private readonly Platform _updatePlatform;
    private readonly IUpdatingClient _updatingClient;

    private bool _suspendAutoCheck;

    public UpdateService(ILogger logger,
        ISettingsService settingsService,
        IMainWindowService mainWindowService,
        IQueuedActionRepository queuedActionRepository,
        IUpdatingClient updatingClient,
        Lazy<IUpdateNotificationProvider> updateNotificationProvider,
        Func<string, ReleaseInstaller> getReleaseInstaller)
    {
        _logger = logger;
        _mainWindowService = mainWindowService;
        _queuedActionRepository = queuedActionRepository;
        _updatingClient = updatingClient;
        _updateNotificationProvider = updateNotificationProvider;
        _getReleaseInstaller = getReleaseInstaller;

        if (OperatingSystem.IsWindows())
            _updatePlatform = Platform.Windows;
        else if (OperatingSystem.IsLinux())
            _updatePlatform = Platform.Linux;
        else if (OperatingSystem.IsMacOS())
            _updatePlatform = Platform.Osx;
        else
            throw new PlatformNotSupportedException("Cannot auto update on the current platform");

        _channel = settingsService.GetSetting("UI.Updating.Channel", "master");
        _autoCheck = settingsService.GetSetting("UI.Updating.AutoCheck", true);
        _autoInstall = settingsService.GetSetting("UI.Updating.AutoInstall", false);
        _autoCheck.SettingChanged += HandleAutoUpdateEvent;
        _mainWindowService.MainWindowOpened += HandleAutoUpdateEvent;
        Timer timer = new(UPDATE_CHECK_INTERVAL);
        timer.Elapsed += HandleAutoUpdateEvent;
        timer.Start();

        _channel.Value = "feature/gh-actions";
        _channel.Save();
        

        InstallQueuedUpdate();
    }

    private void InstallQueuedUpdate()
    {
        if (!_queuedActionRepository.IsTypeQueued("InstallUpdate"))
            return;

        _queuedActionRepository.ClearByType("InstallUpdate");
        _logger.Information("Installing queued update");
        Utilities.ApplyUpdate(false);
    }

    private async Task ShowUpdateDialog(string nextReleaseId)
    {
       
        
        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            // Main window is probably already open but this will bring it into focus
            _mainWindowService.OpenMainWindow();
        });
    }

    private async Task ShowUpdateNotification(string nextReleaseId)
    {
        await _updateNotificationProvider.Value.ShowNotification(nextReleaseId);
    }

    private async Task AutoInstallUpdate(string nextReleaseId)
    {
        ReleaseInstaller installer = _getReleaseInstaller(nextReleaseId);
        await installer.InstallAsync(CancellationToken.None);
        Utilities.ApplyUpdate(true);
    }

    private async void HandleAutoUpdateEvent(object? sender, EventArgs e)
    {
        if (!_autoCheck.Value || _suspendAutoCheck)
            return;

        try
        {
            await CheckForUpdate();
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Auto update failed");
        }
    }
    
    public IGetNextRelease_NextPublishedRelease? CachedLatestRelease { get; private set; }

    public string? CurrentVersion
    {
        get
        {
            object[] attributes = typeof(UpdateService).Assembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false);
            return attributes.Length == 0 ? null : ((AssemblyInformationalVersionAttribute) attributes[0]).InformationalVersion;
        }
    }

    /// <inheritdoc />
    public async Task CacheLatestRelease()
    {
        IOperationResult<IGetNextReleaseResult> result = await _updatingClient.GetNextRelease.ExecuteAsync(CurrentVersion, _channel.Value, _updatePlatform);
        CachedLatestRelease = result.Data?.NextPublishedRelease;
    }
    
    public async Task<bool> CheckForUpdate()
    {
        IOperationResult<IGetNextReleaseResult> result = await _updatingClient.GetNextRelease.ExecuteAsync(CurrentVersion, _channel.Value, _updatePlatform);
        result.EnsureNoErrors();
        
        // Update cache
        CachedLatestRelease = result.Data?.NextPublishedRelease;

        // No update was found
        if (CachedLatestRelease == null)
            return false;

        // Only offer it once per session 
        _suspendAutoCheck = true;

        // If the window is open show the changelog, don't auto-update while the user is busy
        if (_mainWindowService.IsMainWindowOpen)
            await ShowUpdateDialog(CachedLatestRelease.Id);
        else if (!_autoInstall.Value)
            await ShowUpdateNotification(CachedLatestRelease.Id);
        else
            await AutoInstallUpdate(CachedLatestRelease.Id);

        return true;
    }

    /// <inheritdoc />
    public async Task InstallRelease(string releaseId)
    {
        ReleaseInstaller installer = _getReleaseInstaller(releaseId);
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            // Main window is probably already open but this will bring it into focus
            _mainWindowService.OpenMainWindow();
        });
    }

    /// <inheritdoc />
    public void QueueUpdate()
    {
        if (!_queuedActionRepository.IsTypeQueued("InstallUpdate"))
            _queuedActionRepository.Add(new QueuedActionEntity {Type = "InstallUpdate"});
    }

    /// <inheritdoc />
    public ReleaseInstaller GetReleaseInstaller(string releaseId)
    {
        return _getReleaseInstaller(releaseId);
    }
}
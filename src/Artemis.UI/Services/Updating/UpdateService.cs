using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.Storage.Repositories;
using Artemis.Storage.Repositories.Interfaces;
using Artemis.UI.Exceptions;
using Artemis.UI.Services.Interfaces;
using Artemis.UI.Shared.Services.MainWindow;
using Artemis.WebClient.Updating;
using Artemis.WebClient.Workshop.Services;
using Serilog;
using StrawberryShake;
using Timer = System.Timers.Timer;

namespace Artemis.UI.Services.Updating;

public class UpdateService : IUpdateService
{
    private const double UPDATE_CHECK_INTERVAL = 3_600_000; // once per hour
    private readonly PluginSetting<bool> _autoCheck;
    private readonly PluginSetting<bool> _autoInstall;
    private readonly Func<Guid, ReleaseInstaller> _getReleaseInstaller;

    private readonly ILogger _logger;
    private readonly IMainWindowService _mainWindowService;
    private readonly IReleaseRepository _releaseRepository;
    private readonly IWorkshopUpdateService _workshopUpdateService;
    private readonly Lazy<IUpdateNotificationProvider> _updateNotificationProvider;
    private readonly Platform _updatePlatform;
    private readonly IUpdatingClient _updatingClient;

    private bool _suspendAutoCheck;
    private DateTime _lastAutoUpdateCheck;

    public UpdateService(ILogger logger,
        ISettingsService settingsService,
        IMainWindowService mainWindowService,
        IUpdatingClient updatingClient,
        IReleaseRepository releaseRepository,
        IWorkshopUpdateService workshopUpdateService,
        Lazy<IUpdateNotificationProvider> updateNotificationProvider,
        Func<Guid, ReleaseInstaller> getReleaseInstaller)
    {
        _logger = logger;
        _mainWindowService = mainWindowService;
        _updatingClient = updatingClient;
        _releaseRepository = releaseRepository;
        _workshopUpdateService = workshopUpdateService;
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

        _autoCheck = settingsService.GetSetting("UI.Updating.AutoCheck", true);
        _autoInstall = settingsService.GetSetting("UI.Updating.AutoInstall", true);
        _autoCheck.SettingChanged += HandleAutoUpdateEvent;
        mainWindowService.MainWindowOpened += HandleAutoUpdateEvent;
        Timer timer = new(UPDATE_CHECK_INTERVAL);
        timer.Elapsed += HandleAutoUpdateEvent;
        timer.Start();
    }
    
    /// <inheritdoc />
    public string Channel { get; private set; } = "master";

    /// <inheritdoc />
    public string? PreviousVersion { get; private set; }

    /// <inheritdoc />
    public IGetNextRelease_NextPublishedRelease? CachedLatestRelease { get; private set; }
    
    /// <inheritdoc />
    public async Task CacheLatestRelease()
    {
        try
        {
            IOperationResult<IGetNextReleaseResult> result = await _updatingClient.GetNextRelease.ExecuteAsync(Constants.CurrentVersion, Channel, _updatePlatform);
            CachedLatestRelease = result.Data?.NextPublishedRelease;
        }
        catch (Exception e)
        {
            _logger.Warning(e, "Failed to cache latest release");
        }
    }

    /// <inheritdoc />
    public async Task<bool> CheckForUpdate()
    {
        _logger.Information("Performing auto-update check");

        IOperationResult<IGetNextReleaseResult> result = await _updatingClient.GetNextRelease.ExecuteAsync(Constants.CurrentVersion, Channel, _updatePlatform);
        result.EnsureNoErrors();

        // Update cache
        CachedLatestRelease = result.Data?.NextPublishedRelease;

        // No update was found
        if (CachedLatestRelease == null)
            return false;

        // Unless auto install is enabled, only offer it once per session 
        if (!_autoInstall.Value)
            _suspendAutoCheck = true;

        // If the window is open show the changelog, don't auto-update while the user is busy
        if (_mainWindowService.IsMainWindowOpen || !_autoInstall.Value)
        {
            _logger.Information("New update available, offering version {AvailableVersion}", CachedLatestRelease.Version);
            ShowUpdateNotification(CachedLatestRelease);
        }
        else
        {
            _logger.Information("New update available, auto-installing version {AvailableVersion}", CachedLatestRelease.Version);
            await AutoInstallUpdate(CachedLatestRelease);
        }

        return true;
    }

    /// <inheritdoc />
    public ReleaseInstaller GetReleaseInstaller(Guid releaseId)
    {
        return _getReleaseInstaller(releaseId);
    }

    /// <inheritdoc />
    public void RestartForUpdate(string source, bool silent)
    {
        _logger.Information("Restarting for update required by {Source}, silent: {Silent}", source, silent);

        if (!Directory.Exists(Path.Combine(Constants.UpdatingFolder, "pending")))
            throw new ArtemisUIException("Cannot install update, none is pending.");

        Directory.Move(Path.Combine(Constants.UpdatingFolder, "pending"), Path.Combine(Constants.UpdatingFolder, "installing"));
        Utilities.ApplyUpdate(silent);
    }

    /// <inheritdoc />
    public bool Initialize(bool performAutoUpdate)
    {
        if (Constants.CurrentVersion == "local")
            return false;

        string? channelArgument = Constants.StartupArguments.FirstOrDefault(a => a.StartsWith("--channel="));
        if (channelArgument != null)
            Channel = channelArgument.Split("=")[1];
        if (string.IsNullOrWhiteSpace(Channel))
            Channel = "master";

        // There should never be an installing folder
        if (Directory.Exists(Path.Combine(Constants.UpdatingFolder, "installing")))
        {
            _logger.Warning("Cleaning up leftover installing folder, did an update go wrong?");
            try
            {
                Directory.Delete(Path.Combine(Constants.UpdatingFolder, "installing"), true);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Failed to delete leftover installing folder");
            }
        }

        // If an update is pending, don't bother with anything else
        if (Directory.Exists(Path.Combine(Constants.UpdatingFolder, "pending")))
        {
            _logger.Information("Installing pending update");
            try
            {
                RestartForUpdate("PendingFolder", true);
                return true;
            }
            catch (Exception e)
            {
                _logger.Warning(e, "Failed to apply pending update");
                return false;
            }
        }

        ProcessReleaseStatus();

        // Trigger the auto update event so that it doesn't take an hour for the first check to happen
        if (performAutoUpdate)
            HandleAutoUpdateEvent(this, EventArgs.Empty);

        _logger.Information("Update service initialized for {Channel} channel", Channel);
        return false;
    }

    private async Task<bool> AutoCheckForUpdates()
    {
        // Don't perform auto-updates if the current version is local
        if (Constants.CurrentVersion == "local")
            return false;
        
        // Don't perform auto-updates if the setting is disabled or an update was found but not yet installed
        if (!_autoCheck.Value || _suspendAutoCheck)
            return false;

        try
        {
            return await CheckForUpdate() && _autoInstall.Value;
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Auto update-check failed");
        }

        return false;
    }

    private void ProcessReleaseStatus()
    {
        string currentVersion = Constants.CurrentVersion;
        bool updated = _releaseRepository.SaveVersionInstallDate(currentVersion);
        PreviousVersion = _releaseRepository.GetPreviousInstalledVersion()?.Version;

        if (!Directory.Exists(Constants.UpdatingFolder))
            return;

        // Clean up the update folder, leaving only the last ZIP
        foreach (string file in Directory.GetFiles(Constants.UpdatingFolder))
        {
            if (Path.GetExtension(file) != ".zip" || Path.GetFileName(file) == $"{currentVersion}.zip")
                continue;

            try
            {
                _logger.Debug("Cleaning up old update file at {FilePath}", file);
                File.Delete(file);
            }
            catch (Exception e)
            {
                _logger.Warning(e, "Failed to clean up old update file at {FilePath}", file);
            }
        }

        if (updated)
            _updateNotificationProvider.Value.ShowInstalledNotification(currentVersion);
    }

    private void ShowUpdateNotification(IGetNextRelease_NextPublishedRelease release)
    {
        _updateNotificationProvider.Value.ShowNotification(release.Id, release.Version);
    }

    private async Task AutoInstallUpdate(IGetNextRelease_NextPublishedRelease release)
    {
        ReleaseInstaller installer = _getReleaseInstaller(release.Id);
        await installer.InstallAsync(CancellationToken.None);
        RestartForUpdate("AutoInstallUpdate", true);
    }

    private async void HandleAutoUpdateEvent(object? sender, EventArgs e)
    {
        // The event can trigger from multiple sources with a timer acting as a fallback, only actually perform an action once per max 59 minutes
        if (DateTime.UtcNow - _lastAutoUpdateCheck < TimeSpan.FromMinutes(59))
            return;
        
        _lastAutoUpdateCheck = DateTime.UtcNow;
        
        if (await AutoCheckForUpdates())
        {
            _logger.Information("Auto-installing update, not performing workshop update check");
        }
        else
        {
            await _workshopUpdateService.AutoUpdateEntries();
        }
    }
}
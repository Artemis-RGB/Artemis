using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.Storage.Repositories;
using Artemis.UI.Shared.Services.MainWindow;
using Artemis.WebClient.Updating;
using Serilog;
using StrawberryShake;
using Timer = System.Timers.Timer;

namespace Artemis.UI.Services.Updating;

public class UpdateService : IUpdateService
{
    private const double UPDATE_CHECK_INTERVAL = 3_600_000; // once per hour
    private readonly PluginSetting<bool> _autoCheck;
    private readonly PluginSetting<bool> _autoInstall;
    private readonly Platform _updatePlatform;

    private readonly ILogger _logger;
    private readonly IUpdatingClient _updatingClient;
    private readonly IReleaseRepository _releaseRepository;
    private readonly Lazy<IUpdateNotificationProvider> _updateNotificationProvider;
    private readonly Func<string, ReleaseInstaller> _getReleaseInstaller;

    private bool _suspendAutoCheck;

    public UpdateService(ILogger logger,
        ISettingsService settingsService,
        IMainWindowService mainWindowService,
        IUpdatingClient updatingClient,
        IReleaseRepository releaseRepository,
        Lazy<IUpdateNotificationProvider> updateNotificationProvider,
        Func<string, ReleaseInstaller> getReleaseInstaller)
    {
        _logger = logger;
        _updatingClient = updatingClient;
        _releaseRepository = releaseRepository;
        _updateNotificationProvider = updateNotificationProvider;
        _getReleaseInstaller = getReleaseInstaller;

        string? channelArgument = Constants.StartupArguments.FirstOrDefault(a => a.StartsWith("--channel="));
        if (channelArgument != null)
            Channel = channelArgument.Split("=")[1];
        if (string.IsNullOrWhiteSpace(Channel))
            Channel = "master";

        if (OperatingSystem.IsWindows())
            _updatePlatform = Platform.Windows;
        else if (OperatingSystem.IsLinux())
            _updatePlatform = Platform.Linux;
        else if (OperatingSystem.IsMacOS())
            _updatePlatform = Platform.Osx;
        else
            throw new PlatformNotSupportedException("Cannot auto update on the current platform");

        _autoCheck = settingsService.GetSetting("UI.Updating.AutoCheck", true);
        _autoInstall = settingsService.GetSetting("UI.Updating.AutoInstall", false);
        _autoCheck.SettingChanged += HandleAutoUpdateEvent;
        mainWindowService.MainWindowOpened += HandleAutoUpdateEvent;
        Timer timer = new(UPDATE_CHECK_INTERVAL);
        timer.Elapsed += HandleAutoUpdateEvent;
        timer.Start();

        _logger.Information("Update service initialized for {Channel} channel", Channel);
        ProcessReleaseStatus();
    }

    public string Channel { get; }
    public string? PreviousVersion { get; set; }
    public IGetNextRelease_NextPublishedRelease? CachedLatestRelease { get; private set; }

    private void ProcessReleaseStatus()
    {
        // If an update is queued, don't bother with anything else
        string? queued = _releaseRepository.GetQueuedVersion();
        if (queued != null)
        {
            // Remove the queued installation, in case something goes wrong then at least we don't end up in a loop
            _logger.Information("Installing queued version {Version}", queued);
            RestartForUpdate(true);
            return;
        }
        
        // If a different version was installed, mark it as such 
        string? installed = _releaseRepository.GetInstalledVersion();
        if (installed != Constants.CurrentVersion)
            _releaseRepository.FinishInstallation(Constants.CurrentVersion);

        PreviousVersion = _releaseRepository.GetPreviousInstalledVersion();
    }

    private void ShowUpdateNotification(IGetNextRelease_NextPublishedRelease release)
    {
        _updateNotificationProvider.Value.ShowNotification(release.Id, release.Version);
    }

    private async Task AutoInstallUpdate(IGetNextRelease_NextPublishedRelease release)
    {
        ReleaseInstaller installer = _getReleaseInstaller(release.Id);
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
            _logger.Warning(ex, "Auto update-check failed");
        }
    }

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

    public async Task<bool> CheckForUpdate()
    {
        IOperationResult<IGetNextReleaseResult> result = await _updatingClient.GetNextRelease.ExecuteAsync(Constants.CurrentVersion, Channel, _updatePlatform);
        result.EnsureNoErrors();

        // Update cache
        CachedLatestRelease = result.Data?.NextPublishedRelease;

        // No update was found
        if (CachedLatestRelease == null)
            return false;

        // Only offer it once per session 
        _suspendAutoCheck = true;

        // If the window is open show the changelog, don't auto-update while the user is busy
        if (!_autoInstall.Value)
            ShowUpdateNotification(CachedLatestRelease);
        else
            await AutoInstallUpdate(CachedLatestRelease);

        return true;
    }

    /// <inheritdoc />
    public void QueueUpdate(string version)
    {
        _releaseRepository.QueueInstallation(version);
    }
    
    /// <inheritdoc />
    public ReleaseInstaller GetReleaseInstaller(string releaseId)
    {
        return _getReleaseInstaller(releaseId);
    }

    /// <inheritdoc />
    public void RestartForUpdate(bool silent)
    {
        _releaseRepository.DequeueInstallation();
        Utilities.ApplyUpdate(silent);
    }
}
using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Exceptions;
using Artemis.UI.Screens.Settings.Updating;
using Artemis.UI.Services.Interfaces;
using Artemis.UI.Services.Updating;
using Artemis.UI.Shared.Providers;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.MainWindow;
using Artemis.WebClient.Updating;
using Avalonia.Threading;
using DryIoc;
using Serilog;
using StrawberryShake;
using Timer = System.Timers.Timer;

namespace Artemis.UI.Services;

public class UpdateService : IUpdateService
{
    private const double UPDATE_CHECK_INTERVAL = 3_600_000; // once per hour

    private readonly ILogger _logger;
    private readonly IMainWindowService _mainWindowService;
    private readonly IWindowService _windowService;
    private readonly IUpdatingClient _updatingClient;
    private readonly Lazy<IUpdateNotificationProvider> _updateNotificationProvider;
    private readonly Func<string, ReleaseInstaller> _getReleaseInstaller;
    private readonly Platform _updatePlatform;
    private readonly PluginSetting<string> _channel;
    private readonly PluginSetting<bool> _autoCheck;
    private readonly PluginSetting<bool> _autoInstall;

    private bool _suspendAutoCheck;

    public UpdateService(ILogger logger,
        ISettingsService settingsService,
        IMainWindowService mainWindowService,
        IWindowService windowService,
        IUpdatingClient updatingClient,
        Lazy<IUpdateNotificationProvider> updateNotificationProvider,
        Func<string, ReleaseInstaller> getReleaseInstaller)
    {
        _logger = logger;
        _mainWindowService = mainWindowService;
        _windowService = windowService;
        _updatingClient = updatingClient;
        _updateNotificationProvider = updateNotificationProvider;
        _getReleaseInstaller = getReleaseInstaller;

        if (OperatingSystem.IsWindows())
            _updatePlatform = Platform.Windows;
        if (OperatingSystem.IsLinux())
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
    }

    public async Task<bool> CheckForUpdate()
    {
        string? currentVersion = AssemblyProductVersion;
        if (currentVersion == null)
            return false;

        IOperationResult<IGetNextReleaseResult> result = await _updatingClient.GetNextRelease.ExecuteAsync(currentVersion, _channel.Value, _updatePlatform);
        result.EnsureNoErrors();

        // No update was found
        if (result.Data?.NextRelease == null)
            return false;

        // Only offer it once per session 
        _suspendAutoCheck = true;

        // If the window is open show the changelog, don't auto-update while the user is busy
        if (_mainWindowService.IsMainWindowOpen)
            await ShowUpdateDialog(result.Data.NextRelease.Id);
        else if (!_autoInstall.Value)
            await ShowUpdateNotification(result.Data.NextRelease.Id);
        else
            await AutoInstallUpdate(result.Data.NextRelease.Id);

        return true;
    }

    private async Task ShowUpdateDialog(string nextReleaseId)
    {
        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            // Main window is probably already open but this will bring it into focus
            _mainWindowService.OpenMainWindow();
            await _windowService.ShowDialogAsync<UpdateInstallationViewModel>(nextReleaseId);
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

    private static string? AssemblyProductVersion
    {
        get
        {
            object[] attributes = typeof(UpdateService).Assembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false);
            return attributes.Length == 0 ? null : ((AssemblyInformationalVersionAttribute) attributes[0]).InformationalVersion;
        }
    }
}
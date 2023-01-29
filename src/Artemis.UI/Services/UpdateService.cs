using System;
using System.Threading.Tasks;
using System.Timers;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Services.Interfaces;
using Artemis.UI.Shared.Providers;
using Artemis.UI.Shared.Services.MainWindow;
using Avalonia.Threading;
using DryIoc;
using Serilog;

namespace Artemis.UI.Services;

public class UpdateService : IUpdateService
{
    private const double UPDATE_CHECK_INTERVAL = 3_600_000; // once per hour

    private readonly PluginSetting<bool> _autoUpdate;
    private readonly PluginSetting<bool> _checkForUpdates;
    private readonly ILogger _logger;
    private readonly IMainWindowService _mainWindowService;
    private readonly IUpdateProvider? _updateProvider;

    public UpdateService(ILogger logger, IContainer container, ISettingsService settingsService, IMainWindowService mainWindowService)
    {
        _logger = logger;
        _mainWindowService = mainWindowService;

        if (!Constants.BuildInfo.IsLocalBuild)
            _updateProvider = container.Resolve<IUpdateProvider>(IfUnresolved.ReturnDefault);

        _checkForUpdates = settingsService.GetSetting("UI.CheckForUpdates", true);
        _autoUpdate = settingsService.GetSetting("UI.AutoUpdate", false);
        _checkForUpdates.SettingChanged += CheckForUpdatesOnSettingChanged;
        _mainWindowService.MainWindowOpened += WindowServiceOnMainWindowOpened;

        Timer timer = new(UPDATE_CHECK_INTERVAL);
        timer.Elapsed += TimerOnElapsed;
        timer.Start();
    }

    private async void TimerOnElapsed(object? sender, ElapsedEventArgs e)
    {
        await AutoUpdate();
    }

    private async void CheckForUpdatesOnSettingChanged(object? sender, EventArgs e)
    {
        // Run an auto-update as soon as the setting gets changed to enabled
        if (_checkForUpdates.Value)
            await AutoUpdate();
    }

    private async void WindowServiceOnMainWindowOpened(object? sender, EventArgs e)
    {
        await AutoUpdate();
    }

    private async Task AutoUpdate()
    {
        if (_updateProvider == null || !_checkForUpdates.Value || SuspendAutoUpdate)
            return;

        try
        {
            bool updateAvailable = await _updateProvider.CheckForUpdate("master");
            if (!updateAvailable)
                return;

            // Only offer it once per session 
            SuspendAutoUpdate = true;

            // If the window is open show the changelog, don't auto-update while the user is busy
            if (_mainWindowService.IsMainWindowOpen)
            {
                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    // Call OpenMainWindow anyway to focus the main window
                    _mainWindowService.OpenMainWindow();
                    await _updateProvider.OfferUpdate("master", true);
                });
                return;
            }

            // If the window is closed but auto-update is enabled, update silently
            if (_autoUpdate.Value)
                await _updateProvider.ApplyUpdate("master", true);
            // If auto-update is disabled the update provider can show a notification and handle the rest
            else
                await _updateProvider.OfferUpdate("master", false);
        }
        catch (Exception e)
        {
            _logger.Warning(e, "Auto update failed");
        }
    }

    public bool SuspendAutoUpdate { get; set; }
    public bool UpdatingSupported => _updateProvider != null;

    public async Task ManualUpdate()
    {
        if (_updateProvider == null || !_mainWindowService.IsMainWindowOpen)
            return;

        bool updateAvailable = await _updateProvider.CheckForUpdate("master");
        if (!updateAvailable)
            return;

        await _updateProvider.OfferUpdate("master", true);
    }
}
using System;
using System.Linq;
using Artemis.UI.Screens.Settings;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.Builders;
using Artemis.UI.Shared.Services.MainWindow;
using ReactiveUI;

namespace Artemis.UI.Services.Updating;

public class BasicUpdateNotificationProvider : IUpdateNotificationProvider
{
    private readonly Func<IScreen, SettingsViewModel> _getSettingsViewModel;
    private readonly IMainWindowService _mainWindowService;
    private readonly INotificationService _notificationService;
    private Action? _available;
    private Action? _installed;

    public BasicUpdateNotificationProvider(INotificationService notificationService, IMainWindowService mainWindowService, Func<IScreen, SettingsViewModel> getSettingsViewModel)
    {
        _notificationService = notificationService;
        _mainWindowService = mainWindowService;
        _getSettingsViewModel = getSettingsViewModel;
    }

    /// <inheritdoc />
    public void ShowNotification(Guid releaseId, string releaseVersion)
    {
        if (_mainWindowService.IsMainWindowOpen)
            ShowAvailable(releaseVersion);
        else
            _mainWindowService.MainWindowOpened += (_, _) => ShowAvailable(releaseVersion);
    }

    /// <inheritdoc />
    public void ShowInstalledNotification(string installedVersion)
    {
        if (_mainWindowService.IsMainWindowOpen)
            ShowInstalled(installedVersion);
        else
            _mainWindowService.MainWindowOpened += (_, _) => ShowInstalled(installedVersion);
    }

    private void ShowAvailable(string releaseVersion)
    {
        _available?.Invoke();
        _available = _notificationService.CreateNotification()
            .WithTitle("Update available")
            .WithMessage($"Artemis {releaseVersion} has been released")
            .WithSeverity(NotificationSeverity.Success)
            .WithTimeout(TimeSpan.FromSeconds(15))
            .HavingButton(b => b.WithText("View release").WithAction(() => ViewRelease(releaseVersion)))
            .Show();
    }

    private void ShowInstalled(string installedVersion)
    {
        _installed?.Invoke();
        _installed = _notificationService.CreateNotification()
            .WithTitle("Update installed")
            .WithMessage($"Artemis {installedVersion} has been installed.")
            .WithSeverity(NotificationSeverity.Success)
            .WithTimeout(TimeSpan.FromSeconds(15))
            .HavingButton(b => b.WithText("View release").WithAction(() => ViewRelease(installedVersion)))
            .Show();
    }

    private void ViewRelease(string version)
    {
        _installed?.Invoke();
        _available?.Invoke();

        if (_mainWindowService.HostScreen == null)
            return;

        // TODO: When proper routing has been implemented, use that here
        // Create a settings VM to navigate to
        SettingsViewModel settingsViewModel = _getSettingsViewModel(_mainWindowService.HostScreen);
        // Get the release tab
        ReleasesTabViewModel releaseTabViewModel = (ReleasesTabViewModel) settingsViewModel.SettingTabs.First(t => t is ReleasesTabViewModel);

        // Navigate to the settings VM
        _mainWindowService.HostScreen.Router.Navigate.Execute(settingsViewModel);
        // Navigate to the release tab
        releaseTabViewModel.PreselectVersion = version;
        settingsViewModel.SelectedTab = releaseTabViewModel;
    }
}
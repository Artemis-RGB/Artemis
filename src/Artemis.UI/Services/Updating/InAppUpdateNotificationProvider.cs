using System;
using System.Linq;
using System.Threading.Tasks;
using Artemis.UI.Screens.Settings;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.Builders;
using Artemis.UI.Shared.Services.MainWindow;
using ReactiveUI;

namespace Artemis.UI.Services.Updating;

public class InAppUpdateNotificationProvider : IUpdateNotificationProvider
{
    private readonly Func<IScreen, SettingsViewModel> _getSettingsViewModel;
    private readonly IMainWindowService _mainWindowService;
    private readonly INotificationService _notificationService;
    private Action? _notification;

    public InAppUpdateNotificationProvider(INotificationService notificationService, IMainWindowService mainWindowService, Func<IScreen, SettingsViewModel> getSettingsViewModel)
    {
        _notificationService = notificationService;
        _mainWindowService = mainWindowService;
        _getSettingsViewModel = getSettingsViewModel;
    }

    private void ShowInAppNotification(Guid releaseId, string releaseVersion)
    {
        _notification?.Invoke();
        _notification = _notificationService.CreateNotification()
            .WithTitle("Update available")
            .WithMessage($"Artemis version {releaseVersion} has been released")
            .WithSeverity(NotificationSeverity.Success)
            .WithTimeout(TimeSpan.FromSeconds(15))
            .HavingButton(b => b.WithText("View release").WithAction(() => ViewRelease(releaseId)))
            .Show();
    }

    private void ViewRelease(Guid releaseId)
    {
        _notification?.Invoke();

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
        releaseTabViewModel.PreselectId = releaseId;
        settingsViewModel.SelectedTab = releaseTabViewModel;
    }

    /// <inheritdoc />
    public void ShowNotification(Guid releaseId, string releaseVersion)
    {
        if (_mainWindowService.IsMainWindowOpen)
            ShowInAppNotification(releaseId, releaseVersion);
        else
            _mainWindowService.MainWindowOpened += (_, _) => ShowInAppNotification(releaseId, releaseVersion);
    }
}
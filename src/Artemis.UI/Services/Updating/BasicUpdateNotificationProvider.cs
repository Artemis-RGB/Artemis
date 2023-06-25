using System;
using System.Threading.Tasks;
using Artemis.UI.Shared.Routing;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.Builders;
using Artemis.UI.Shared.Services.MainWindow;

namespace Artemis.UI.Services.Updating;

public class BasicUpdateNotificationProvider : IUpdateNotificationProvider
{
    private readonly IMainWindowService _mainWindowService;
    private readonly INotificationService _notificationService;
    private readonly IRouter _router;
    private Action? _available;
    private Action? _installed;

    public BasicUpdateNotificationProvider(INotificationService notificationService, IMainWindowService mainWindowService, IRouter router)
    {
        _notificationService = notificationService;
        _mainWindowService = mainWindowService;
        _router = router;
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
            .HavingButton(b => b.WithText("View release").WithAction(async () => await ViewRelease(installedVersion)))
            .Show();
    }

    private async Task ViewRelease(string version)
    {
        _installed?.Invoke();
        _available?.Invoke();

        await _router.Navigate($"settings/releases/{version}");
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
}
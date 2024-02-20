using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Notifications;
using Artemis.UI.Screens.Settings;
using Artemis.UI.Services.Updating;
using Artemis.UI.Shared.Routing;
using Artemis.UI.Shared.Services.MainWindow;
using Avalonia.Threading;
using Microsoft.Toolkit.Uwp.Notifications;
using ReactiveUI;

namespace Artemis.UI.Windows.Providers;

public class WindowsUpdateNotificationProvider : IUpdateNotificationProvider
{
    private readonly Func<Guid, ReleaseInstaller> _getReleaseInstaller;
    private readonly IMainWindowService _mainWindowService;
    private readonly IUpdateService _updateService;
    private readonly IRouter _router;
    private CancellationTokenSource? _cancellationTokenSource;

    public WindowsUpdateNotificationProvider(IMainWindowService mainWindowService, IUpdateService updateService, IRouter router, Func<Guid, ReleaseInstaller> getReleaseInstaller)
    {
        _mainWindowService = mainWindowService;
        _updateService = updateService;
        _router = router;
        _getReleaseInstaller = getReleaseInstaller;
        ToastNotificationManagerCompat.OnActivated += ToastNotificationManagerCompatOnOnActivated;
    }

    /// <inheritdoc />
    public void ShowNotification(Guid releaseId, string releaseVersion)
    {
        GetBuilderForRelease(releaseId, releaseVersion)
            .AddText("Update available")
            .AddText($"Artemis {releaseVersion} has been released")
            .AddButton(new ToastButton()
                .SetContent("Install")
                .AddArgument("action", "install").SetAfterActivationBehavior(ToastAfterActivationBehavior.PendingUpdate))
            .AddButton(new ToastButton().SetContent("View changes").AddArgument("action", "view-changes"))
            .Show(t => t.Tag = releaseId.ToString());
    }

    /// <inheritdoc />
    public void ShowInstalledNotification(string installedVersion)
    {
        new ToastContentBuilder().AddArgument("releaseVersion", installedVersion)
            .AddText("Update installed")
            .AddText($"Artemis {installedVersion} has been installed")
            .AddButton(new ToastButton().SetContent("View changes").AddArgument("action", "view-changes"))
            .Show();
    }

    private void ViewRelease(Guid? releaseId)
    {
        Dispatcher.UIThread.Invoke(async () =>
        {
            _mainWindowService.OpenMainWindow();
            if (releaseId != null && releaseId.Value != Guid.Empty)
                await _router.Navigate($"settings/releases/{releaseId}");
            else
                await _router.Navigate("settings/releases");
        });
    }

    private async Task InstallRelease(Guid releaseId, string releaseVersion)
    {
        ReleaseInstaller installer = _getReleaseInstaller(releaseId);
        void InstallerOnPropertyChanged(object? sender, PropertyChangedEventArgs e) => UpdateInstallProgress(releaseId, installer);

        GetBuilderForRelease(releaseId, releaseVersion)
            .AddAudio(new ToastAudio {Silent = true})
            .AddText("Installing Artemis update")
            .AddVisualChild(new AdaptiveProgressBar()
            {
                Title = releaseVersion,
                Value = new BindableProgressBarValue("progressValue"),
                Status = new BindableString("progressStatus")
            })
            .AddButton(new ToastButton().SetContent("Cancel").AddArgument("action", "cancel"))
            .Show(t =>
            {
                t.Tag = releaseId.ToString();
                t.Data = GetDataForInstaller(installer);
            });

        // Wait for Windows animations to catch up to us, we fast!
        await Task.Delay(2000);
        _cancellationTokenSource = new CancellationTokenSource();
        installer.PropertyChanged += InstallerOnPropertyChanged;
        try
        {
            await installer.InstallAsync(_cancellationTokenSource.Token);
        }
        catch (Exception)
        {
            if (_cancellationTokenSource.IsCancellationRequested)
                return;
            throw;
        }
        finally
        {
            installer.PropertyChanged -= InstallerOnPropertyChanged;
        }

        // If the main window is not open the user isn't busy, restart straight away
        if (!_mainWindowService.IsMainWindowOpen)
        {
            _updateService.RestartForUpdate("WindowsNotification", true);
            return;
        }

        // Ask for a restart because the user is actively using Artemis
        GetBuilderForRelease(releaseId, releaseVersion)
            .AddAudio(new ToastAudio {Silent = true})
            .AddText("Update ready")
            .AddText("Artemis must restart to finish the update")
            .AddButton(new ToastButton().SetContent("Restart Artemis").AddArgument("action", "restart-for-update"))
            .AddButton(new ToastButton().SetContent("Later").AddArgument("action", "postpone-update"))
            .Show(t => t.Tag = releaseId.ToString());
    }

    private void UpdateInstallProgress(Guid releaseId, ReleaseInstaller installer)
    {
        ToastNotificationManagerCompat.CreateToastNotifier().Update(GetDataForInstaller(installer), releaseId.ToString());
    }

    private ToastContentBuilder GetBuilderForRelease(Guid releaseId, string releaseVersion)
    {
        return new ToastContentBuilder().AddArgument("releaseId", releaseId.ToString()).AddArgument("releaseVersion", releaseVersion);
    }

    private NotificationData GetDataForInstaller(ReleaseInstaller installer)
    {
        NotificationData data = new()
        {
            Values =
            {
                ["progressValue"] = (installer.Progress / 100f).ToString(CultureInfo.InvariantCulture),
                ["progressStatus"] = installer.Status
            }
        };

        return data;
    }

    private async void ToastNotificationManagerCompatOnOnActivated(ToastNotificationActivatedEventArgsCompat e)
    {
        ToastArguments args = ToastArguments.Parse(e.Argument);

        Guid releaseId = args.Contains("releaseId") ? Guid.Parse(args.Get("releaseId")) : Guid.Empty;
        string releaseVersion = args.Get("releaseVersion");
        string action = "view-changes";
        if (args.Contains("action"))
            action = args.Get("action");

        if (action == "install")
            await InstallRelease(releaseId, releaseVersion);
        else if (action == "view-changes")
            ViewRelease(releaseId);
        else if (action == "cancel")
            _cancellationTokenSource?.Cancel();
        else if (action == "restart-for-update")
            _updateService.RestartForUpdate("WindowsNotification", false);
    }
}
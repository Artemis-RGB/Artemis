using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Notifications;
using Artemis.UI.Screens.Settings;
using Artemis.UI.Services.Updating;
using Artemis.UI.Shared.Services.MainWindow;
using Avalonia.Threading;
using Microsoft.Toolkit.Uwp.Notifications;
using ReactiveUI;

namespace Artemis.UI.Windows.Providers;

public class WindowsUpdateNotificationProvider : IUpdateNotificationProvider
{
    private readonly Func<string, ReleaseInstaller> _getReleaseInstaller;
    private readonly Func<IScreen, SettingsViewModel> _getSettingsViewModel;
    private readonly IMainWindowService _mainWindowService;
    private readonly IUpdateService _updateService;

    public WindowsUpdateNotificationProvider(IMainWindowService mainWindowService,
        IUpdateService updateService,
        Func<IScreen, SettingsViewModel> getSettingsViewModel,
        Func<string, ReleaseInstaller> getReleaseInstaller)
    {
        _mainWindowService = mainWindowService;
        _updateService = updateService;
        _getSettingsViewModel = getSettingsViewModel;
        _getReleaseInstaller = getReleaseInstaller;
        ToastNotificationManagerCompat.OnActivated += ToastNotificationManagerCompatOnOnActivated;
    }

    private async void ToastNotificationManagerCompatOnOnActivated(ToastNotificationActivatedEventArgsCompat e)
    {
        ToastArguments args = ToastArguments.Parse(e.Argument);
        string releaseId = args.Get("releaseId");
        string releaseVersion = args.Get("releaseVersion");
        string action = "view-changes";
        if (args.Contains("action"))
            action = args.Get("action");

        if (action == "install")
            await InstallRelease(releaseId, releaseVersion);
        else if (action == "view-changes")
            ViewRelease(releaseId);
        else if (action == "restart-for-update")
            _updateService.RestartForUpdate(false);
    }

    public async Task ShowNotification(string releaseId, string releaseVersion)
    {
        new ToastContentBuilder()
            .AddArgument("releaseId", releaseId)
            .AddArgument("releaseVersion", releaseVersion)
            .AddText("Update available")
            .AddText($"Artemis version {releaseVersion} has been released")
            .AddButton(new ToastButton()
                .SetContent("Install")
                .AddArgument("action", "install").SetAfterActivationBehavior(ToastAfterActivationBehavior.PendingUpdate))
            .AddButton(new ToastButton().SetContent("View changes").AddArgument("action", "view-changes"))
            .Show(t => t.Tag = releaseId);
    }

    private void ViewRelease(string releaseId)
    {
        Dispatcher.UIThread.Post(() =>
        {
            _mainWindowService.OpenMainWindow();

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
        });
    }

    private async Task InstallRelease(string releaseId, string releaseVersion)
    {
        ReleaseInstaller installer = _getReleaseInstaller(releaseId);
        void InstallerOnPropertyChanged(object? sender, PropertyChangedEventArgs e) => UpdateInstallProgress(releaseId, installer);

        new ToastContentBuilder()
            .AddArgument("releaseId", releaseId)
            .AddArgument("releaseVersion", releaseVersion)
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
                t.Tag = releaseId;
                t.Data = GetInstallerNotificationData(installer);
            });

        await Task.Delay(2000);
        installer.PropertyChanged += InstallerOnPropertyChanged;
        await installer.InstallAsync(CancellationToken.None);
        installer.PropertyChanged -= InstallerOnPropertyChanged;

        _updateService.QueueUpdate();
        
        new ToastContentBuilder()
            .AddArgument("releaseId", releaseId)
            .AddArgument("releaseVersion", releaseVersion)
            .AddAudio(new ToastAudio {Silent = true})
            .AddText("Update ready")
            .AddText($"Artemis version {releaseVersion} is ready to be applied")
            .AddButton(new ToastButton().SetContent("Restart Artemis").AddArgument("action", "restart-for-update"))
            .AddButton(new ToastButton().SetContent("Later").AddArgument("action", "postpone-update"))
            .Show(t => t.Tag = releaseId);
    }

    private void UpdateInstallProgress(string releaseId, ReleaseInstaller installer)
    {
        ToastNotificationManagerCompat.CreateToastNotifier().Update(GetInstallerNotificationData(installer), releaseId);
    }

    private NotificationData GetInstallerNotificationData(ReleaseInstaller installer)
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
}
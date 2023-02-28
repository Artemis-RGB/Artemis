using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.UI.Extensions;
using Artemis.UI.Services.Updating;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.Builders;
using Artemis.WebClient.Updating;
using ReactiveUI;
using Serilog;
using StrawberryShake;

namespace Artemis.UI.Screens.Settings.Updating;

public class ReleaseViewModel : ActivatableViewModelBase
{
    private readonly ILogger _logger;
    private readonly INotificationService _notificationService;
    private readonly IUpdateService _updateService;
    private readonly Platform _updatePlatform;
    private readonly IUpdatingClient _updatingClient;
    private readonly IWindowService _windowService;
    private CancellationTokenSource? _installerCts;
    private string _changelog = string.Empty;
    private string _commit = string.Empty;
    private string _shortCommit = string.Empty;
    private long _fileSize;
    private bool _installationAvailable;
    private bool _installationFinished;
    private bool _installationInProgress;
    private bool _loading = true;
    private bool _retrievedDetails;

    public ReleaseViewModel(string releaseId,
        string version,
        DateTimeOffset createdAt,
        ILogger logger,
        IUpdatingClient updatingClient,
        INotificationService notificationService,
        IUpdateService updateService,
        IWindowService windowService)
    {
        _logger = logger;
        _updatingClient = updatingClient;
        _notificationService = notificationService;
        _updateService = updateService;
        _windowService = windowService;

        if (OperatingSystem.IsWindows())
            _updatePlatform = Platform.Windows;
        else if (OperatingSystem.IsLinux())
            _updatePlatform = Platform.Linux;
        else if (OperatingSystem.IsMacOS())
            _updatePlatform = Platform.Osx;
        else
            throw new PlatformNotSupportedException("Cannot auto update on the current platform");

        
        ReleaseId = releaseId;
        Version = version;
        CreatedAt = createdAt;
        ReleaseInstaller = updateService.GetReleaseInstaller(ReleaseId);

        Install = ReactiveCommand.CreateFromTask(ExecuteInstall);
        Restart = ReactiveCommand.Create(ExecuteRestart);
        CancelInstall = ReactiveCommand.Create(() => _installerCts?.Cancel());

        this.WhenActivated(d =>
        {
            // There's no point in running anything but the latest version of the current channel.
            // Perhaps later that won't be true anymore, then we could consider allowing to install
            // older versions with compatible database versions.
            InstallationAvailable = _updateService.CachedLatestRelease?.Id == ReleaseId;
            RetrieveDetails(d.AsCancellationToken()).ToObservable();
            Disposable.Create(_installerCts, cts => cts?.Cancel()).DisposeWith(d);
        });
    }

    public string ReleaseId { get; }

    private void ExecuteRestart()
    {
        _updateService.RestartForUpdate(false);
    }

    public ReactiveCommand<Unit, Unit> Restart { get; set; }
    public ReactiveCommand<Unit, Unit> Install { get; }
    public ReactiveCommand<Unit, Unit> CancelInstall { get; }

    public string Version { get; }
    public DateTimeOffset CreatedAt { get; }
    public ReleaseInstaller ReleaseInstaller { get; }

    public string Changelog
    {
        get => _changelog;
        set => RaiseAndSetIfChanged(ref _changelog, value);
    }

    public string Commit
    {
        get => _commit;
        set => RaiseAndSetIfChanged(ref _commit, value);
    }

    public string ShortCommit
    {
        get => _shortCommit;
        set => RaiseAndSetIfChanged(ref _shortCommit, value);
    }

    public long FileSize
    {
        get => _fileSize;
        set => RaiseAndSetIfChanged(ref _fileSize, value);
    }

    public bool Loading
    {
        get => _loading;
        private set => RaiseAndSetIfChanged(ref _loading, value);
    }

    public bool InstallationAvailable
    {
        get => _installationAvailable;
        set => RaiseAndSetIfChanged(ref _installationAvailable, value);
    }

    public bool InstallationInProgress
    {
        get => _installationInProgress;
        set => RaiseAndSetIfChanged(ref _installationInProgress, value);
    }

    public bool InstallationFinished
    {
        get => _installationFinished;
        set => RaiseAndSetIfChanged(ref _installationFinished, value);
    }

    public bool IsCurrentVersion => Version == Constants.CurrentVersion;

    public void NavigateToSource()
    {
        Utilities.OpenUrl($"https://github.com/Artemis-RGB/Artemis/commit/{Commit}");
    }

    private async Task ExecuteInstall(CancellationToken cancellationToken)
    {
        _installerCts = new CancellationTokenSource();
        try
        {
            InstallationInProgress = true;
            await ReleaseInstaller.InstallAsync(_installerCts.Token);
            _updateService.QueueUpdate();
            InstallationFinished = true;
        }
        catch (Exception e)
        {
            if (_installerCts.IsCancellationRequested)
                return;
            _windowService.ShowExceptionDialog("Failed to install update", e);
        }
        finally
        {
            InstallationInProgress = false;
        }
    }

    private async Task RetrieveDetails(CancellationToken cancellationToken)
    {
        if (_retrievedDetails)
            return;

        try
        {
            Loading = true;

            IOperationResult<IGetReleaseByIdResult> result = await _updatingClient.GetReleaseById.ExecuteAsync(ReleaseId, cancellationToken);
            IGetReleaseById_PublishedRelease? release = result.Data?.PublishedRelease;
            if (release == null)
                return;

            Changelog = release.Changelog;
            Commit = release.Commit;
            ShortCommit = release.Commit.Substring(0, 7);
            FileSize = release.Artifacts.FirstOrDefault(a => a.Platform == _updatePlatform)?.FileInfo.DownloadSize ?? 0;

            _retrievedDetails = true;
        }
        catch (Exception e)
        {
            _logger.Warning(e, "Failed to retrieve release details");
            _notificationService.CreateNotification()
                .WithTitle("Failed to retrieve details")
                .WithMessage(e.Message)
                .WithSeverity(NotificationSeverity.Warning)
                .Show();
        }
        finally
        {
            Loading = false;
        }
    }
}
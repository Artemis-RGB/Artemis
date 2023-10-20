using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.UI.Services.Updating;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Routing;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.Builders;
using Artemis.WebClient.Updating;
using PropertyChanged.SourceGenerator;
using ReactiveUI;
using Serilog;
using StrawberryShake;

namespace Artemis.UI.Screens.Settings.Updating;

public partial class ReleaseDetailsViewModel : RoutableScreen<ReleaseDetailsViewModelParameters>
{
    private readonly ObservableAsPropertyHelper<long> _fileSize;
    private readonly ILogger _logger;
    private readonly INotificationService _notificationService;
    private readonly IUpdateService _updateService;
    private readonly IUpdatingClient _updatingClient;
    private CancellationTokenSource? _installerCts;
    [Notify(Setter.Private)] private bool _loading = true;
    [Notify] private IGetReleaseById_PublishedRelease? _release;
    [Notify] private ReleaseInstaller? _releaseInstaller;
    [Notify] private bool _installationAvailable;
    [Notify] private bool _installationFinished;
    [Notify] private bool _installationInProgress;

    public ReleaseDetailsViewModel(ILogger logger, IUpdatingClient updatingClient, INotificationService notificationService, IUpdateService updateService)
    {
        _logger = logger;
        _updatingClient = updatingClient;
        _notificationService = notificationService;
        _updateService = updateService;

        Platform updatePlatform;
        if (OperatingSystem.IsWindows())
            updatePlatform = Platform.Windows;
        else if (OperatingSystem.IsLinux())
            updatePlatform = Platform.Linux;
        else if (OperatingSystem.IsMacOS())
            updatePlatform = Platform.Osx;
        else
            throw new PlatformNotSupportedException("Cannot auto update on the current platform");

        Install = ReactiveCommand.CreateFromTask(ExecuteInstall);
        Restart = ReactiveCommand.Create(ExecuteRestart);
        CancelInstall = ReactiveCommand.Create(() => _installerCts?.Cancel());

        _fileSize = this.WhenAnyValue(vm => vm.Release)
            .Select(release => release?.Artifacts.FirstOrDefault(a => a.Platform == updatePlatform)?.FileInfo.DownloadSize ?? 0)
            .ToProperty(this, vm => vm.FileSize);

        this.WhenActivated(d => Disposable.Create(_installerCts, cts => cts?.Cancel()).DisposeWith(d));
    }

    public ReactiveCommand<Unit, Unit> Restart { get; }
    public ReactiveCommand<Unit, Unit> Install { get; }
    public ReactiveCommand<Unit, Unit> CancelInstall { get; }

    public long FileSize => _fileSize.Value;
    
    public void NavigateToSource()
    {
        if (Release != null)
            Utilities.OpenUrl($"https://github.com/Artemis-RGB/Artemis/commit/{Release.Commit}");
    }

    /// <inheritdoc />
    public override async Task OnNavigating(ReleaseDetailsViewModelParameters parameters, NavigationArguments args, CancellationToken cancellationToken)
    {
        // There's no point in running anything but the latest version of the current channel.
        // Perhaps later that won't be true anymore, then we could consider allowing to install
        // older versions with compatible database versions.
        InstallationAvailable = _updateService.CachedLatestRelease?.Id == parameters.ReleaseId;
        await RetrieveDetails(parameters.ReleaseId, cancellationToken);
        ReleaseInstaller = _updateService.GetReleaseInstaller(parameters.ReleaseId);
    }

    private void ExecuteRestart()
    {
        _updateService.RestartForUpdate(false);
    }

    private async Task ExecuteInstall(CancellationToken cancellationToken)
    {
        if (ReleaseInstaller == null)
            return;

        _installerCts = new CancellationTokenSource();
        try
        {
            InstallationInProgress = true;
            await ReleaseInstaller.InstallAsync(_installerCts.Token);
            InstallationFinished = true;
        }
        catch (Exception e)
        {
            if (_installerCts.IsCancellationRequested)
                return;

            _logger.Warning(e, "Failed to install update through UI");
            _notificationService.CreateNotification()
                .WithTitle("Failed to install update")
                .WithMessage(e.Message)
                .WithSeverity(NotificationSeverity.Warning)
                .Show();
        }
        finally
        {
            InstallationInProgress = false;
        }
    }

    private async Task RetrieveDetails(Guid releaseId, CancellationToken cancellationToken)
    {
        try
        {
            Loading = true;

            IOperationResult<IGetReleaseByIdResult> result = await _updatingClient.GetReleaseById.ExecuteAsync(releaseId, cancellationToken);
            IGetReleaseById_PublishedRelease? release = result.Data?.PublishedRelease;
            if (release == null)
                return;

            Release = release;
        }
        catch (TaskCanceledException)
        {
            // ignored
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

public class ReleaseDetailsViewModelParameters
{
    public Guid ReleaseId { get; set; }
}
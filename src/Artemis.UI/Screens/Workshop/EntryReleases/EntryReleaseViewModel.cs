using System;
using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.Screens.Workshop.Parameters;
using Artemis.UI.Shared.Routing;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.Builders;
using Artemis.UI.Shared.Utilities;
using Artemis.WebClient.Workshop;
using Artemis.WebClient.Workshop.Handlers.InstallationHandlers;
using PropertyChanged.SourceGenerator;
using StrawberryShake;

namespace Artemis.UI.Screens.Workshop.EntryReleases;

public partial class EntryReleaseViewModel : RoutableScreen<ReleaseDetailParameters>
{
    private readonly IWorkshopClient _client;
    private readonly IRouter _router;
    private readonly INotificationService _notificationService;
    private readonly IWindowService _windowService;
    private readonly EntryInstallationHandlerFactory _factory;
    private readonly Progress<StreamProgress> _progress = new();

    [Notify] private IGetReleaseById_Release? _release;
    [Notify] private float _installProgress;
    [Notify] private bool _installationInProgress;

    private CancellationTokenSource? _cts;

    public EntryReleaseViewModel(IWorkshopClient client, IRouter router, INotificationService notificationService, IWindowService windowService, EntryInstallationHandlerFactory factory)
    {
        _client = client;
        _router = router;
        _notificationService = notificationService;
        _windowService = windowService;
        _factory = factory;
        _progress.ProgressChanged += (_, f) => InstallProgress = f.ProgressPercentage;
    }

    public async Task Close()
    {
        await _router.GoUp();
    }

    public async Task Install()
    {
        if (Release == null)
            return;

        _cts = new CancellationTokenSource();
        InstallProgress = 0;
        InstallationInProgress = true;
        try
        {
            IEntryInstallationHandler handler = _factory.CreateHandler(Release.Entry.EntryType);
            EntryInstallResult result = await handler.InstallAsync(Release.Entry, Release, _progress, _cts.Token);
            if (result.IsSuccess)
                _notificationService.CreateNotification().WithTitle("Installation succeeded").WithSeverity(NotificationSeverity.Success).Show();
            else if (!_cts.IsCancellationRequested)
                _notificationService.CreateNotification().WithTitle("Installation failed").WithMessage(result.Message).WithSeverity(NotificationSeverity.Error).Show();
        }
        catch (Exception e)
        {
            _windowService.ShowExceptionDialog("Failed to install workshop entry", e);
        }
        finally
        {
            InstallationInProgress = false;
        }
    }

    public void Cancel()
    {
        _cts?.Cancel();
    }

    /// <inheritdoc />
    public override async Task OnNavigating(ReleaseDetailParameters parameters, NavigationArguments args, CancellationToken cancellationToken)
    {
        IOperationResult<IGetReleaseByIdResult> result = await _client.GetReleaseById.ExecuteAsync(parameters.ReleaseId, cancellationToken);
        Release = result.Data?.Release;
    }

    #region Overrides of RoutableScreen

    /// <inheritdoc />
    public override Task OnClosing(NavigationArguments args)
    {
        if (!InstallationInProgress)
            return Task.CompletedTask;

        args.Cancel();
        _notificationService.CreateNotification().WithMessage("Please wait for the installation to finish").Show();
        return Task.CompletedTask;
    }

    #endregion
}
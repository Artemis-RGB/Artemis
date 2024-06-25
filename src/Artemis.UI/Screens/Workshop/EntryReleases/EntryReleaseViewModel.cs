using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.Screens.Workshop.Parameters;
using Artemis.UI.Shared.Routing;
using Artemis.UI.Shared.Services;
using Artemis.WebClient.Workshop;
using PropertyChanged.SourceGenerator;
using StrawberryShake;

namespace Artemis.UI.Screens.Workshop.EntryReleases;

public partial class EntryReleaseViewModel : RoutableScreen<ReleaseDetailParameters>
{
    private readonly IWorkshopClient _client;
    private readonly INotificationService _notificationService;

    [Notify] private IGetReleaseById_Release? _release;

    public EntryReleaseViewModel(IWorkshopClient client, INotificationService notificationService, EntryReleaseInfoViewModel entryReleaseInfoViewModel)
    {
        EntryReleaseInfoViewModel = entryReleaseInfoViewModel;
        
        _client = client;
        _notificationService = notificationService;
    }
    
    public EntryReleaseInfoViewModel EntryReleaseInfoViewModel { get; }

    /// <inheritdoc />
    public override async Task OnNavigating(ReleaseDetailParameters parameters, NavigationArguments args, CancellationToken cancellationToken)
    {
        IOperationResult<IGetReleaseByIdResult> result = await _client.GetReleaseById.ExecuteAsync(parameters.ReleaseId, cancellationToken);
        
        Release = result.Data?.Release;
        EntryReleaseInfoViewModel.Release = Release;
    }

    #region Overrides of RoutableScreen

    /// <inheritdoc />
    public override Task OnClosing(NavigationArguments args)
    {
        if (!EntryReleaseInfoViewModel.InstallationInProgress)
            return Task.CompletedTask;

        args.Cancel();
        _notificationService.CreateNotification().WithMessage("Please wait for the installation to finish").Show();
        return Task.CompletedTask;
    }

    #endregion
}
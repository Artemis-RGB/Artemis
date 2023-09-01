
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Routing;
using Artemis.WebClient.Workshop;
using Artemis.WebClient.Workshop.Services;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.Home;

public class WorkshopOfflineViewModel : RoutableScreen<ActivatableViewModelBase, WorkshopOfflineParameters>, IWorkshopViewModel
{
    private readonly IRouter _router;
    private readonly IWorkshopService _workshopService;
    private string _message;

    /// <inheritdoc />
    public WorkshopOfflineViewModel(IWorkshopService workshopService, IRouter router)
    {
        _workshopService = workshopService;
        _router = router;

        Retry = ReactiveCommand.CreateFromTask(ExecuteRetry);
    }

    public ReactiveCommand<Unit, Unit> Retry { get; }

    public string Message
    {
        get => _message;
        set => RaiseAndSetIfChanged(ref _message, value);
    }

    public override Task OnNavigating(WorkshopOfflineParameters parameters, NavigationArguments args, CancellationToken cancellationToken)
    {
        Message = parameters.Message;
        return base.OnNavigating(parameters, args, cancellationToken);
    }

    private async Task ExecuteRetry(CancellationToken cancellationToken)
    {
        IWorkshopService.WorkshopStatus status = await _workshopService.GetWorkshopStatus(cancellationToken);
        if (status.IsReachable)
            await _router.Navigate("workshop");

        Message = status.Message;
    }

    public EntryType? EntryType => null;
}

public class WorkshopOfflineParameters
{
    public string Message { get; set; }
}
using System;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.Shared.Routing;
using Artemis.WebClient.Workshop.Services;
using PropertyChanged.SourceGenerator;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.Home;

public partial class WorkshopOfflineViewModel : RoutableScreen<WorkshopOfflineParameters>
{
    private readonly IRouter _router;
    private readonly IWorkshopService _workshopService;
    [Notify] private string _message = string.Empty;

    /// <inheritdoc />
    public WorkshopOfflineViewModel(IWorkshopService workshopService, IRouter router)
    {
        _workshopService = workshopService;
        _router = router;

        Retry = ReactiveCommand.CreateFromTask(ExecuteRetry);
    }

    public ReactiveCommand<Unit, Unit> Retry { get; }

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
}

public class WorkshopOfflineParameters
{
    public string Message { get; set; } = string.Empty;
}
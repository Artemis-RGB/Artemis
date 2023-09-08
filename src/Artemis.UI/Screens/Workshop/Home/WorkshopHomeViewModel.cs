using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.Extensions;
using Artemis.UI.Screens.Workshop.SubmissionWizard;
using Artemis.UI.Shared.Routing;
using Artemis.UI.Shared.Services;
using Artemis.WebClient.Workshop.Services;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.Home;

public class WorkshopHomeViewModel : RoutableScreen
{
    private readonly IWindowService _windowService;
    private bool _workshopReachable;

    public WorkshopHomeViewModel(IRouter router, IWindowService windowService, IWorkshopService workshopService)
    {
        _windowService = windowService;

        AddSubmission = ReactiveCommand.CreateFromTask(ExecuteAddSubmission, this.WhenAnyValue(vm => vm.WorkshopReachable));
        Navigate = ReactiveCommand.CreateFromTask<string>(async r => await router.Navigate(r), this.WhenAnyValue(vm => vm.WorkshopReachable));

        this.WhenActivatedAsync(async d => WorkshopReachable = await workshopService.ValidateWorkshopStatus(d.AsCancellationToken()));
    }

#if DEBUG
    public bool ShowLayouts => true;
# else
    public bool ShowLayouts => false;
#endif
    
    public ReactiveCommand<Unit, Unit> AddSubmission { get; }
    public ReactiveCommand<string, Unit> Navigate { get; }

    public bool WorkshopReachable
    {
        get => _workshopReachable;
        private set => RaiseAndSetIfChanged(ref _workshopReachable, value);
    }

    private async Task ExecuteAddSubmission(CancellationToken arg)
    {
        await _windowService.ShowDialogAsync<SubmissionWizardViewModel>();
    }
}
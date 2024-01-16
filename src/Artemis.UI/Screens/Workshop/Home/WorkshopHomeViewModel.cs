using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.Extensions;
using Artemis.UI.Screens.Workshop.SubmissionWizard;
using Artemis.UI.Shared.Routing;
using Artemis.UI.Shared.Services;
using Artemis.WebClient.Workshop.Services;
using PropertyChanged.SourceGenerator;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.Home;

public partial class WorkshopHomeViewModel : RoutableScreen
{
    private readonly IWindowService _windowService;
    [Notify(Setter.Private)] private bool _workshopReachable;

    public WorkshopHomeViewModel(IRouter router, IWindowService windowService, IWorkshopService workshopService)
    {
        _windowService = windowService;

        AddSubmission = ReactiveCommand.CreateFromTask(ExecuteAddSubmission, this.WhenAnyValue(vm => vm.WorkshopReachable));
        Navigate = ReactiveCommand.CreateFromTask<string>(async r => await router.Navigate(r), this.WhenAnyValue(vm => vm.WorkshopReachable));

        this.WhenActivatedAsync(async d => WorkshopReachable = await workshopService.ValidateWorkshopStatus(d.AsCancellationToken()));
    }
    
    public ReactiveCommand<Unit, Unit> AddSubmission { get; }
    public ReactiveCommand<string, Unit> Navigate { get; }

    private async Task ExecuteAddSubmission(CancellationToken arg)
    {
        await _windowService.ShowDialogAsync<SubmissionWizardViewModel>();
    }
}
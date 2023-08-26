using System.Reactive;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.Screens.Workshop.SubmissionWizard;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Routing;
using Artemis.UI.Shared.Services;
using Artemis.WebClient.Workshop;
using Artemis.WebClient.Workshop.Services;
using Avalonia.Threading;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.Home;

public class WorkshopHomeViewModel : ActivatableViewModelBase, IWorkshopViewModel
{
    private readonly IWindowService _windowService;
    private readonly IWorkshopService _workshopService;
    private bool _workshopReachable;

    public WorkshopHomeViewModel(IRouter router, IWindowService windowService, IWorkshopService workshopService)
    {
        _windowService = windowService;
        _workshopService = workshopService;

        AddSubmission = ReactiveCommand.CreateFromTask(ExecuteAddSubmission, this.WhenAnyValue(vm => vm.WorkshopReachable));
        Navigate = ReactiveCommand.CreateFromTask<string>(async r => await router.Navigate(r), this.WhenAnyValue(vm => vm.WorkshopReachable));

        this.WhenActivated((CompositeDisposable _) => Dispatcher.UIThread.InvokeAsync(ValidateWorkshopStatus));
    }

    public ReactiveCommand<Unit, Unit> AddSubmission { get; }
    public ReactiveCommand<string, Unit> Navigate { get; }

    public bool WorkshopReachable
    {
        get => _workshopReachable;
        private set => RaiseAndSetIfChanged(ref _workshopReachable, value);
    }

    private async Task ExecuteAddSubmission(CancellationToken arg)
    {
        await _windowService.ShowDialogAsync<SubmissionWizardViewModel, bool>();
    }

    private async Task ValidateWorkshopStatus()
    {
        WorkshopReachable = await _workshopService.ValidateWorkshopStatus();
    }

    public EntryType? EntryType => null;
}
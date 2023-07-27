using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.Screens.Workshop.SubmissionWizard;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Routing;
using Artemis.UI.Shared.Services;
using Artemis.WebClient.Workshop;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.Home;

public class WorkshopHomeViewModel : ActivatableViewModelBase, IWorkshopViewModel
{
    private readonly IWindowService _windowService;

    public WorkshopHomeViewModel(IRouter router, IWindowService windowService)
    {
        _windowService = windowService;
        AddSubmission = ReactiveCommand.CreateFromTask(ExecuteAddSubmission);
        Navigate = ReactiveCommand.CreateFromTask<string>(async r => await router.Navigate(r));
    }

    public ReactiveCommand<Unit, Unit> AddSubmission { get; }
    public ReactiveCommand<string, Unit> Navigate { get; }

    private async Task ExecuteAddSubmission(CancellationToken arg)
    {
        await _windowService.ShowDialogAsync<SubmissionWizardViewModel>();
    }

    public EntryType? EntryType => null;
}
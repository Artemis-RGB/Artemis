using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.Screens.Workshop.Parameters;
using Artemis.UI.Screens.Workshop.SubmissionWizard;
using Artemis.UI.Shared.Routing;
using Artemis.UI.Shared.Services;
using Artemis.WebClient.Workshop;
using Artemis.WebClient.Workshop.Services;
using PropertyChanged.SourceGenerator;
using ReactiveUI;
using StrawberryShake;

namespace Artemis.UI.Screens.Workshop.Library;

public partial class SubmissionManagementViewModel : RoutableHostScreen<RoutableScreen, WorkshopDetailParameters>
{
    private readonly IWorkshopClient _client;
    private readonly IWindowService _windowService;
    private readonly IRouter _router;
    private readonly IWorkshopService _workshopService;
    private readonly SubmissionDetailsViewModel _detailsViewModel;

    [Notify] private IGetSubmittedEntryById_Entry? _entry;
    [Notify] private List<IGetSubmittedEntryById_Entry_Releases>? _releases;
    [Notify] private IGetSubmittedEntryById_Entry_Releases? _selectedRelease;

    public SubmissionManagementViewModel(IWorkshopClient client, IRouter router, IWindowService windowService, IWorkshopService workshopService, SubmissionDetailsViewModel detailsViewModel)
    {
        _detailsViewModel = detailsViewModel;
        _client = client;
        _router = router;
        _windowService = windowService;
        _workshopService = workshopService;

        RecycleScreen = false;
        
        this.WhenActivated(d =>
        {
            this.WhenAnyValue(vm => vm.SelectedRelease)
                .WhereNotNull()
                .Subscribe(r => _router.Navigate($"workshop/library/submissions/{Entry?.Id}/releases/{r.Id}"))
                .DisposeWith(d);
        });
    }

    public override RoutableScreen DefaultScreen => _detailsViewModel;

    public async Task ViewWorkshopPage()
    {
        if (Entry != null)
            await _workshopService.NavigateToEntry(Entry.Id, Entry.EntryType);
    }

    public async Task CreateRelease()
    {
        if (Entry != null)
            await _windowService.ShowDialogAsync<ReleaseWizardViewModel>(Entry);
    }

    public async Task DeleteSubmission()
    {
        if (Entry == null)
            return;

        bool confirmed = await _windowService.ShowConfirmContentDialog(
            "Delete submission?",
            "You cannot undo this by yourself.\r\n" +
            "Users that have already downloaded your submission will keep it.");
        if (!confirmed)
            return;

        IOperationResult<IRemoveEntryResult> result = await _client.RemoveEntry.ExecuteAsync(Entry.Id);
        result.EnsureNoErrors();
        await _router.Navigate("workshop/library/submissions");
    }

    public override async Task OnNavigating(WorkshopDetailParameters parameters, NavigationArguments args, CancellationToken cancellationToken)
    {
        // If there is a 2nd parameter, it's a release ID
        SelectedRelease = args.RouteParameters.Length > 1 ? Releases?.FirstOrDefault(r => r.Id == (long) args.RouteParameters[1]) : null;
        
        // OnNavigating may just be getting called to update the selected release
        if (Entry?.Id == parameters.EntryId)
        {
            // Reapply the entry when closing a release, this is mainly because the entry icon probably got disposed
            if (SelectedRelease == null)
                await _detailsViewModel.SetEntry(Entry, cancellationToken);
            
            // No need to reload the entry since it's the same
            return;
        }

        IOperationResult<IGetSubmittedEntryByIdResult> result = await _client.GetSubmittedEntryById.ExecuteAsync(parameters.EntryId, cancellationToken);
        if (result.IsErrorResult())
            return;

        Entry = result.Data?.Entry;
        Releases = Entry?.Releases.OrderByDescending(r => r.CreatedAt).ToList();

        await _detailsViewModel.SetEntry(Entry, cancellationToken);
    }

    public override async Task OnClosing(NavigationArguments args)
    {
        await _detailsViewModel.OnClosing(args);
    }
}
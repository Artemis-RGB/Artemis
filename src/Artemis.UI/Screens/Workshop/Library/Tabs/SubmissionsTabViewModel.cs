using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.Extensions;
using Artemis.UI.Screens.Workshop.CurrentUser;
using Artemis.UI.Screens.Workshop.SubmissionWizard;
using Artemis.UI.Shared.Routing;
using Artemis.UI.Shared.Services;
using Artemis.WebClient.Workshop;
using Artemis.WebClient.Workshop.Services;
using DynamicData;
using ReactiveUI;
using StrawberryShake;

namespace Artemis.UI.Screens.Workshop.Library.Tabs;

public class SubmissionsTabViewModel : RoutableScreen
{
    private readonly IWorkshopClient _client;
    private readonly SourceCache<IGetSubmittedEntries_SubmittedEntries, long> _entries;
    private readonly IWindowService _windowService;
    private bool _isLoading = true;
    private bool _workshopReachable;

    public SubmissionsTabViewModel(IWorkshopClient client,
        IAuthenticationService authenticationService,
        IWindowService windowService, 
        IWorkshopService workshopService, 
        Func<IGetSubmittedEntries_SubmittedEntries, SubmissionsTabItemViewModel> getSubmissionsTabItemViewModel)
    {
        _client = client;
        _windowService = windowService;
        _entries = new SourceCache<IGetSubmittedEntries_SubmittedEntries, long>(e => e.Id);
        _entries.Connect()
            .Transform(getSubmissionsTabItemViewModel)
            .Bind(out ReadOnlyObservableCollection<SubmissionsTabItemViewModel> entries)
            .Subscribe();

        AddSubmission = ReactiveCommand.CreateFromTask(ExecuteAddSubmission, this.WhenAnyValue(vm => vm.WorkshopReachable));
        Login = ReactiveCommand.CreateFromTask(ExecuteLogin, this.WhenAnyValue(vm => vm.WorkshopReachable));

        IsLoggedIn = authenticationService.IsLoggedIn;
        Entries = entries;

        this.WhenActivatedAsync(async d =>
        {
            WorkshopReachable = await workshopService.ValidateWorkshopStatus(d.AsCancellationToken());
            if (WorkshopReachable)
                await GetEntries(d.AsCancellationToken());
        });
    }

    public ReactiveCommand<Unit, Unit> Login { get; }
    public ReactiveCommand<Unit, Unit> AddSubmission { get; }

    public IObservable<bool> IsLoggedIn { get; }
    public ReadOnlyObservableCollection<SubmissionsTabItemViewModel> Entries { get; }

    public bool WorkshopReachable
    {
        get => _workshopReachable;
        set => RaiseAndSetIfChanged(ref _workshopReachable, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => RaiseAndSetIfChanged(ref _isLoading, value);
    }

    private async Task ExecuteLogin(CancellationToken ct)
    {
        await _windowService.CreateContentDialog().WithViewModel(out WorkshopLoginViewModel _).WithTitle("Workshop login").ShowAsync();
        await GetEntries(ct);
    }

    private async Task ExecuteAddSubmission(CancellationToken arg)
    {
        await _windowService.ShowDialogAsync<SubmissionWizardViewModel>();
    }
    
    private async Task GetEntries(CancellationToken ct)
    {
        IsLoading = true;

        try
        {
            IOperationResult<IGetSubmittedEntriesResult> result = await _client.GetSubmittedEntries.ExecuteAsync(null, ct);

            if (result.Data?.SubmittedEntries == null)
                _entries.Clear();
            else
                _entries.Edit(e =>
                {
                    e.Clear();
                    e.AddOrUpdate(result.Data.SubmittedEntries);
                });
        }
        finally
        {
            IsLoading = false;
        }
    }
}
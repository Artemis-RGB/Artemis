using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.Extensions;
using Artemis.UI.Screens.Workshop.CurrentUser;
using Artemis.UI.Screens.Workshop.SubmissionWizard;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Routing;
using Artemis.UI.Shared.Services;
using Artemis.WebClient.Workshop;
using Artemis.WebClient.Workshop.Services;
using DynamicData;
using ReactiveUI;
using StrawberryShake;

namespace Artemis.UI.Screens.Workshop.Library.Tabs;

public class LibrarySubmissionsViewModel : ActivatableViewModelBase, IWorkshopViewModel
{
    private readonly IWorkshopClient _client;
    private readonly SourceCache<IGetSubmittedEntries_SubmittedEntries, Guid> _entries;
    private readonly IWindowService _windowService;
    private bool _isLoading = true;
    private bool _workshopReachable;

    public LibrarySubmissionsViewModel(IWorkshopClient client, IAuthenticationService authenticationService, IWindowService windowService, IWorkshopService workshopService, IRouter router)
    {
        _client = client;
        _windowService = windowService;
        _entries = new SourceCache<IGetSubmittedEntries_SubmittedEntries, Guid>(e => e.Id);
        _entries.Connect().Bind(out ReadOnlyObservableCollection<IGetSubmittedEntries_SubmittedEntries> entries).Subscribe();

        AddSubmission = ReactiveCommand.CreateFromTask(ExecuteAddSubmission, this.WhenAnyValue(vm => vm.WorkshopReachable));
        Login = ReactiveCommand.CreateFromTask(ExecuteLogin, this.WhenAnyValue(vm => vm.WorkshopReachable));
        NavigateToEntry = ReactiveCommand.CreateFromTask<IGetSubmittedEntries_SubmittedEntries>(ExecuteNavigateToEntry);

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
    public ReactiveCommand<IGetSubmittedEntries_SubmittedEntries, Unit> NavigateToEntry { get; }

    public IObservable<bool> IsLoggedIn { get; }
    public ReadOnlyObservableCollection<IGetSubmittedEntries_SubmittedEntries> Entries { get; }

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
    }

    private async Task ExecuteAddSubmission(CancellationToken arg)
    {
        await _windowService.ShowDialogAsync<SubmissionWizardViewModel, bool>();
    }

    private Task ExecuteNavigateToEntry(IGetSubmittedEntries_SubmittedEntries entry, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
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

    public EntryType? EntryType => null;
}
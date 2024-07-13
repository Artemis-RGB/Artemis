using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
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
using DynamicData.Aggregation;
using DynamicData.Binding;
using Humanizer;
using PropertyChanged.SourceGenerator;
using ReactiveUI;
using StrawberryShake;

namespace Artemis.UI.Screens.Workshop.Library.Tabs;

public partial class SubmissionsTabViewModel : RoutableScreen
{
    private readonly IWorkshopClient _client;
    private readonly SourceCache<IGetSubmittedEntries_SubmittedEntries, long> _entries;
    private readonly IWindowService _windowService;
    private readonly ObservableAsPropertyHelper<bool> _empty;
    
    [Notify] private bool _isLoading = true;
    [Notify] private bool _workshopReachable;
    [Notify] private string? _searchEntryInput;
    
    public SubmissionsTabViewModel(IWorkshopClient client,
        IAuthenticationService authenticationService,
        IWindowService windowService, 
        IWorkshopService workshopService, 
        Func<IGetSubmittedEntries_SubmittedEntries, SubmissionsTabItemViewModel> getSubmissionsTabItemViewModel)
    {
        IObservable<Func<IGetSubmittedEntries_SubmittedEntries, bool>> searchFilter = this.WhenAnyValue(vm => vm.SearchEntryInput).Throttle(TimeSpan.FromMilliseconds(100)).Select(CreatePredicate);
        
        _client = client;
        _windowService = windowService;
        _entries = new SourceCache<IGetSubmittedEntries_SubmittedEntries, long>(e => e.Id);
        _entries.Connect()
            .Filter(searchFilter)
            .Sort(SortExpressionComparer<IGetSubmittedEntries_SubmittedEntries>.Descending(p => p.CreatedAt))
            .Transform(getSubmissionsTabItemViewModel)
            .GroupWithImmutableState(vm => vm.Entry.EntryType.Humanize(LetterCasing.Title).Pluralize())
            .Bind(out ReadOnlyObservableCollection<IGrouping<SubmissionsTabItemViewModel, long, string>> entries)
            .Subscribe();
        _empty = _entries.Connect().Count().Select(c => c == 0).ToProperty(this, vm => vm.Empty);
        
        AddSubmission = ReactiveCommand.CreateFromTask(ExecuteAddSubmission, this.WhenAnyValue(vm => vm.WorkshopReachable));
        Login = ReactiveCommand.CreateFromTask(ExecuteLogin, this.WhenAnyValue(vm => vm.WorkshopReachable));

        IsLoggedIn = authenticationService.IsLoggedIn;
        EntryGroups = entries;

        this.WhenActivatedAsync(async d =>
        {
            WorkshopReachable = await workshopService.ValidateWorkshopStatus(d.AsCancellationToken());
            if (WorkshopReachable)
                await GetEntries(d.AsCancellationToken());
        });
    }

    public bool Empty => _empty.Value;
    public ReactiveCommand<Unit, Unit> Login { get; }
    public ReactiveCommand<Unit, Unit> AddSubmission { get; }
    public IObservable<bool> IsLoggedIn { get; }
    public ReadOnlyObservableCollection<IGrouping<SubmissionsTabItemViewModel, long, string>> EntryGroups { get; }
    
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
    
    private Func<IGetSubmittedEntries_SubmittedEntries, bool> CreatePredicate(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return _ => true;

        return data => data.Name.Contains(text, StringComparison.InvariantCultureIgnoreCase);
    }
}
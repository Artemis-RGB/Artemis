using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.Screens.Workshop.Categories;
using Artemis.UI.Screens.Workshop.Parameters;
using Artemis.UI.Shared.Routing;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.Builders;
using Artemis.WebClient.Workshop;
using Avalonia.Threading;
using DynamicData;
using PropertyChanged.SourceGenerator;
using ReactiveUI;
using StrawberryShake;

namespace Artemis.UI.Screens.Workshop.Entries.List;

public abstract partial class EntryListViewModel : RoutableScreen<WorkshopListParameters>
{
    private readonly SourceList<IEntrySummary> _entries = new();
    private readonly ObservableAsPropertyHelper<bool> _isLoading;
    private readonly INotificationService _notificationService;
    private readonly string _route;
    private readonly ObservableAsPropertyHelper<bool> _showPagination;
    private readonly IWorkshopClient _workshopClient;
    [Notify] private int _page;
    [Notify] private int _loadedPage = -1;
    [Notify] private int _totalPages = 1;

    protected EntryListViewModel(string route,
        IWorkshopClient workshopClient,
        IRouter router,
        CategoriesViewModel categoriesViewModel,
        EntryListInputViewModel entryListInputViewModel,
        INotificationService notificationService,
        Func<IEntrySummary, EntryListItemViewModel> getEntryListViewModel)
    {
        _route = route;
        _workshopClient = workshopClient;
        _notificationService = notificationService;
        _showPagination = this.WhenAnyValue(vm => vm.TotalPages).Select(t => t > 1).ToProperty(this, vm => vm.ShowPagination);
        _isLoading = this.WhenAnyValue(vm => vm.Page, vm => vm.LoadedPage, (p, c) => p != c).ToProperty(this, vm => vm.IsLoading);

        CategoriesViewModel = categoriesViewModel;
        InputViewModel = entryListInputViewModel;

        _entries.Connect()
            .ObserveOn(new AvaloniaSynchronizationContext(DispatcherPriority.SystemIdle))
            .Transform(getEntryListViewModel)
            .Bind(out ReadOnlyObservableCollection<EntryListItemViewModel> entries)
            .Subscribe();
        Entries = entries;

        // Respond to page changes
        this.WhenAnyValue<EntryListViewModel, int>(vm => vm.Page).Skip(1).Subscribe(p => Task.Run(() => router.Navigate($"{_route}/{p}")));

        this.WhenActivated(d =>
        {
            // Respond to filter query input changes
            InputViewModel.WhenAnyValue(vm => vm.Search).Skip(1).Throttle(TimeSpan.FromMilliseconds(200)).Subscribe(_ => RefreshToStart()).DisposeWith(d);
            InputViewModel.WhenAnyValue(vm => vm.SortBy, vm => vm.EntriesPerPage).Skip(1).Subscribe(_ => RefreshToStart()).DisposeWith(d);
            CategoriesViewModel.WhenAnyValue(vm => vm.CategoryFilters).Skip(1).Subscribe(_ => RefreshToStart()).DisposeWith(d);
        });
    }

    public bool ShowPagination => _showPagination.Value;
    public bool IsLoading => _isLoading.Value;

    public CategoriesViewModel CategoriesViewModel { get; }
    public EntryListInputViewModel InputViewModel { get; }

    public ReadOnlyObservableCollection<EntryListItemViewModel> Entries { get; }
    
    public override async Task OnNavigating(WorkshopListParameters parameters, NavigationArguments args, CancellationToken cancellationToken)
    {
        Page = Math.Max(1, parameters.Page);

        await Task.Delay(200, cancellationToken);
        if (!cancellationToken.IsCancellationRequested)
            await Query(cancellationToken);
    }

    public override Task OnClosing(NavigationArguments args)
    {
        // Clear search if not navigating to a child
        if (!args.Path.StartsWith(_route))
            InputViewModel.ClearLastSearch();
        return base.OnClosing(args);
    }


    protected virtual EntryFilterInput GetFilter()
    {
        return new EntryFilterInput {And = CategoriesViewModel.CategoryFilters};
    }

    protected virtual IReadOnlyList<EntrySortInput> GetSort()
    {
        // Sort by created at
        if (InputViewModel.SortBy == 1)
            return new[] {new EntrySortInput {CreatedAt = SortEnumType.Desc}};

        // Sort by downloads
        if (InputViewModel.SortBy == 2)
            return new[] {new EntrySortInput {Downloads = SortEnumType.Desc}};


        // Sort by latest release, then by created at
        return new[]
        {
            new EntrySortInput {LatestRelease = new ReleaseSortInput {CreatedAt = SortEnumType.Desc}},
            new EntrySortInput {CreatedAt = SortEnumType.Desc}
        };
    }

    private void RefreshToStart()
    {
        // Reset to page one, will trigger a query
        if (Page != 1)
            Page = 1;
        // If already at page one, force a query
        else
            Task.Run(() => Query(CancellationToken.None));
    }

    private async Task Query(CancellationToken cancellationToken)
    {
        try
        {
            string? search = string.IsNullOrWhiteSpace(InputViewModel.Search) ? null : InputViewModel.Search;
            EntryFilterInput filter = GetFilter();
            IReadOnlyList<EntrySortInput> sort = GetSort();
            IOperationResult<IGetEntriesResult> entries = await _workshopClient.GetEntries.ExecuteAsync(
                search,
                filter,
                InputViewModel.EntriesPerPage * (Page - 1),
                InputViewModel.EntriesPerPage,
                sort,
                cancellationToken
            );
            entries.EnsureNoErrors();

            if (entries.Data?.Entries?.Items != null)
            {
                TotalPages = (int) Math.Ceiling(entries.Data.Entries.TotalCount / (double) InputViewModel.EntriesPerPage);
                InputViewModel.TotalCount = entries.Data.Entries.TotalCount;
                _entries.Edit(e =>
                {
                    e.Clear();
                    e.AddRange(entries.Data.Entries.Items);
                });
            }
            else
            {
                TotalPages = 1;
            }
        }
        catch (Exception e)
        {
            _notificationService.CreateNotification()
                .WithTitle("Failed to load entries")
                .WithMessage(e.Message)
                .WithSeverity(NotificationSeverity.Error)
                .Show();
        }
        finally
        {
            LoadedPage = Page;
        }
    }
}
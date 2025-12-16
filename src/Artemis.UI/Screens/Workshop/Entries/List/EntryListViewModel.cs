using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.Extensions;
using Artemis.UI.Screens.Workshop.Categories;
using Artemis.UI.Shared.Routing;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.Builders;
using Artemis.WebClient.Workshop;
using DynamicData;
using PropertyChanged.SourceGenerator;
using ReactiveUI;
using StrawberryShake;
using Vector = Avalonia.Vector;

namespace Artemis.UI.Screens.Workshop.Entries.List;

public partial class EntryListViewModel : RoutableScreen
{
    private readonly SourceList<IEntrySummary> _entries = new();
    private readonly INotificationService _notificationService;
    private readonly IWorkshopClient _workshopClient;
    private IGetEntries_EntriesV2_PageInfo? _currentPageInfo;

    [Notify] private bool _initializing = true;
    [Notify] private bool _fetchingMore;
    [Notify] private int _entriesPerFetch;
    [Notify] private bool _includeDefaultEntries;
    [Notify] private Vector _scrollOffset;

    protected EntryListViewModel(EntryType entryType,
        IWorkshopClient workshopClient,
        EntryListInputViewModel entryListInputViewModel,
        INotificationService notificationService,
        Func<EntryType, CategoriesViewModel> getCategoriesViewModel,
        Func<IEntrySummary, EntryListItemViewModel> getEntryListViewModel)
    {
        _workshopClient = workshopClient;
        _notificationService = notificationService;

        CategoriesViewModel = getCategoriesViewModel(entryType);
        InputViewModel = entryListInputViewModel;
        EntryType = entryType;
        
        _entries.Connect()
            .Transform(getEntryListViewModel)
            .Bind(out ReadOnlyObservableCollection<EntryListItemViewModel> entries)
            .Subscribe();
        Entries = entries;

        // Respond to filter query input changes
        this.WhenAnyValue(vm => vm.IncludeDefaultEntries).Skip(1).Throttle(TimeSpan.FromMilliseconds(200)).Subscribe(_ => Reset());
        this.WhenActivated(d =>
        {
            InputViewModel.WhenAnyValue(vm => vm.Search).Skip(1).Throttle(TimeSpan.FromMilliseconds(200)).Subscribe(_ => Reset()).DisposeWith(d);
            InputViewModel.WhenAnyValue(vm => vm.SortBy).Skip(1).Throttle(TimeSpan.FromMilliseconds(200)).Subscribe(_ => Reset()).DisposeWith(d);
            CategoriesViewModel.WhenAnyValue(vm => vm.CategoryFilters).Skip(1).Subscribe(_ => Reset()).DisposeWith(d);
        });

        // Load entries when the view model is first activated
        this.WhenActivatedAsync(async _ =>
        {
            if (_entries.Count == 0)
            {
                await Task.Delay(250);
                await FetchMore(CancellationToken.None);
                Initializing = false;
            }
        });
    }

    public CategoriesViewModel CategoriesViewModel { get; }
    public EntryListInputViewModel InputViewModel { get; }
    public bool ShowCategoryFilter { get; set; } = true;
    public EntryType EntryType { get; }

    public ReadOnlyObservableCollection<EntryListItemViewModel> Entries { get; }

    public async Task FetchMore(CancellationToken cancellationToken)
    {
        if (FetchingMore || _currentPageInfo != null && !_currentPageInfo.HasNextPage)
            return;

        FetchingMore = true;

        int entriesPerFetch = _entries.Count == 0 ? _entriesPerFetch * 2 : _entriesPerFetch;
        string? search = string.IsNullOrWhiteSpace(InputViewModel.Search) ? null : InputViewModel.Search;
        EntryFilterInput filter = GetFilter();
        IReadOnlyList<EntrySortInput> sort = GetSort();

        try
        {
            IOperationResult<IGetEntriesResult> entries = await _workshopClient.GetEntries.ExecuteAsync(search, IncludeDefaultEntries, filter, sort, entriesPerFetch, _currentPageInfo?.EndCursor, cancellationToken);
            entries.EnsureNoErrors();

            _currentPageInfo = entries.Data?.EntriesV2?.PageInfo;
            if (entries.Data?.EntriesV2?.Edges != null)
                _entries.Edit(e => e.AddRange(entries.Data.EntriesV2.Edges.Select(edge => edge.Node)));

            InputViewModel.TotalCount = entries.Data?.EntriesV2?.TotalCount ?? 0;
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
            FetchingMore = false;
        }
    }

    private EntryFilterInput GetFilter()
    {
        return new EntryFilterInput
        {
            And =
            [
                new EntryFilterInput {EntryType = new EntryTypeOperationFilterInput {Eq = EntryType}},
                ..CategoriesViewModel.CategoryFilters ?? []
            ]
        };
    }

    private IReadOnlyList<EntrySortInput> GetSort()
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

    private void Reset()
    {
        _entries.Clear();
        _currentPageInfo = null;
        Task.Run(() => FetchMore(CancellationToken.None));
    }
}
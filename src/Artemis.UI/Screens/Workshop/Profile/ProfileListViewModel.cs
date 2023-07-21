using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.Screens.Workshop.Categories;
using Artemis.UI.Screens.Workshop.Entries;
using Artemis.UI.Screens.Workshop.Parameters;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Routing;
using Artemis.WebClient.Workshop;
using ReactiveUI;
using StrawberryShake;

namespace Artemis.UI.Screens.Workshop.Profile;

public class ProfileListViewModel : RoutableScreen<ActivatableViewModelBase, WorkshopListParameters>, IWorkshopViewModel
{
    private readonly IRouter _router;
    private readonly IWorkshopClient _workshopClient;
    private readonly ObservableAsPropertyHelper<bool> _showPagination;
    private List<EntryListViewModel>? _entries;
    private int _page;
    private int _totalPages = 1;
    private int _entriesPerPage = 5;

    public ProfileListViewModel(IWorkshopClient workshopClient, IRouter router, CategoriesViewModel categoriesViewModel)
    {
        _workshopClient = workshopClient;
        _router = router;
        _showPagination = this.WhenAnyValue(vm => vm.TotalPages).Select(t => t > 1).ToProperty(this, vm => vm.ShowPagination);
        
        CategoriesViewModel = categoriesViewModel;

        // Respond to page changes
        this.WhenAnyValue(vm => vm.Page).Skip(1).Subscribe(p => Task.Run(() => _router.Navigate($"workshop/profiles/{p}")));
        // Respond to filter changes
        this.WhenActivated(d => CategoriesViewModel.WhenAnyValue(vm => vm.CategoryFilters).Skip(1).Subscribe(_ =>
        {
            // Reset to page one, will trigger a query
            if (Page != 1)
                Page = 1;
            // If already at page one, force a query
            else
                Task.Run(() => Query(CancellationToken.None));
        }).DisposeWith(d));
    }

    public bool ShowPagination => _showPagination.Value;
    public CategoriesViewModel CategoriesViewModel { get; }

    public List<EntryListViewModel>? Entries
    {
        get => _entries;
        set => RaiseAndSetIfChanged(ref _entries, value);
    }

    public int Page
    {
        get => _page;
        set => RaiseAndSetIfChanged(ref _page, value);
    }

    public int TotalPages
    {
        get => _totalPages;
        set => RaiseAndSetIfChanged(ref _totalPages, value);
    }

    public int EntriesPerPage
    {
        get => _entriesPerPage;
        set => RaiseAndSetIfChanged(ref _entriesPerPage, value);
    }

    public override async Task OnNavigating(WorkshopListParameters parameters, NavigationArguments args, CancellationToken cancellationToken)
    {
        Page = Math.Max(1, parameters.Page);
        
        // Throttle page changes
        await Task.Delay(200, cancellationToken);
        
        if (!cancellationToken.IsCancellationRequested)
            await Query(cancellationToken);
    }

    private async Task Query(CancellationToken cancellationToken)
    {
        EntryFilterInput filter = GetFilter();
        IOperationResult<IGetEntriesResult> entries = await _workshopClient.GetEntries.ExecuteAsync(filter, EntriesPerPage * (Page - 1), EntriesPerPage, cancellationToken);
        if (!entries.IsErrorResult() && entries.Data?.Entries?.Items != null)
        {
            Entries = entries.Data.Entries.Items.Select(n => new EntryListViewModel(n, _router)).ToList();
            TotalPages = (int) Math.Ceiling(entries.Data.Entries.TotalCount / (double) EntriesPerPage);
        }
        else
            TotalPages = 1;
    }

    private EntryFilterInput GetFilter()
    {
        EntryFilterInput filter = new()
        {
            EntryType = new EntryTypeOperationFilterInput {Eq = WebClient.Workshop.EntryType.Profile},
            And = CategoriesViewModel.CategoryFilters
        };

        return filter;
    }

    public EntryType? EntryType => null;
}
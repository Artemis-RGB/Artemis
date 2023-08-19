using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.Builders;
using Artemis.WebClient.Workshop;
using Avalonia.Threading;
using DryIoc.ImTools;
using DynamicData;
using ReactiveUI;
using StrawberryShake;

namespace Artemis.UI.Screens.Workshop.Profile;

public class ProfileListViewModel : RoutableScreen<ActivatableViewModelBase, WorkshopListParameters>, IWorkshopViewModel
{
    private readonly INotificationService _notificationService;
    private readonly Func<IGetEntries_Entries_Items, EntryListViewModel> _getEntryListViewModel;
    private readonly IWorkshopClient _workshopClient;
    private readonly ObservableAsPropertyHelper<bool> _showPagination;
    private readonly ObservableAsPropertyHelper<bool> _isLoading;
    private SourceList<IGetEntries_Entries_Items> _entries = new();
    private int _page;
    private int _loadedPage = -1;
    private int _totalPages = 1;
    private int _entriesPerPage = 10;

    public ProfileListViewModel(IWorkshopClient workshopClient,
        IRouter router, 
        CategoriesViewModel categoriesViewModel, 
        INotificationService notificationService,
        Func<IGetEntries_Entries_Items, EntryListViewModel> getEntryListViewModel)
    {
        _workshopClient = workshopClient;
        _notificationService = notificationService;
        _getEntryListViewModel = getEntryListViewModel;
        _showPagination = this.WhenAnyValue(vm => vm.TotalPages).Select(t => t > 1).ToProperty(this, vm => vm.ShowPagination);
        _isLoading = this.WhenAnyValue(vm => vm.Page, vm => vm.LoadedPage, (p, c) => p != c).ToProperty(this, vm => vm.IsLoading);

        CategoriesViewModel = categoriesViewModel;
        
        _entries.Connect()
            .ObserveOn(new AvaloniaSynchronizationContext(DispatcherPriority.SystemIdle))
            .Transform(getEntryListViewModel)
            .Bind(out ReadOnlyObservableCollection<EntryListViewModel> entries)
            .Subscribe();
        Entries = entries;
        
        // Respond to page changes
        this.WhenAnyValue(vm => vm.Page).Skip(1).Subscribe(p => Task.Run(() => router.Navigate($"workshop/profiles/{p}")));
        
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
    public bool IsLoading => _isLoading.Value;

    public CategoriesViewModel CategoriesViewModel { get; }

    public ReadOnlyObservableCollection<EntryListViewModel> Entries { get; }

    public int Page
    {
        get => _page;
        set => RaiseAndSetIfChanged(ref _page, value);
    }

    public int LoadedPage
    {
        get => _loadedPage;
        set => RaiseAndSetIfChanged(ref _loadedPage, value);
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

        // Throttle page changes, wait longer for the first one to keep UI smooth
        // if (Entries == null)
            // await Task.Delay(400, cancellationToken);
        // else
            await Task.Delay(200, cancellationToken);

        if (!cancellationToken.IsCancellationRequested)
            await Query(cancellationToken);
    }

    private async Task Query(CancellationToken cancellationToken)
    {
        try
        {
            EntryFilterInput filter = GetFilter();
            IOperationResult<IGetEntriesResult> entries = await _workshopClient.GetEntries.ExecuteAsync(filter, EntriesPerPage * (Page - 1), EntriesPerPage, cancellationToken);
            entries.EnsureNoErrors();

            if (entries.Data?.Entries?.Items != null)
            {
                TotalPages = (int) Math.Ceiling(entries.Data.Entries.TotalCount / (double) EntriesPerPage);
                _entries.Edit(e =>
                {
                    e.Clear();
                    e.AddRange(entries.Data.Entries.Items);
                });
            }
            else
                TotalPages = 1;
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
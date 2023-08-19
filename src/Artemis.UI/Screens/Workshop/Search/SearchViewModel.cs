using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.Screens.Workshop.CurrentUser;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Routing;
using Artemis.WebClient.Workshop;
using Artemis.WebClient.Workshop.Services;
using ReactiveUI;
using Serilog;
using StrawberryShake;

namespace Artemis.UI.Screens.Workshop.Search;

public class SearchViewModel : ViewModelBase
{
    private readonly ILogger _logger;
    private readonly IRouter _router;
    private readonly IWorkshopClient _workshopClient;
    private readonly IWorkshopService _workshopService;
    private EntryType? _entryType;
    private bool _isLoading;
    private SearchResultViewModel? _selectedEntry;

    public SearchViewModel(ILogger logger, IWorkshopClient workshopClient, IWorkshopService workshopService, IRouter router, CurrentUserViewModel currentUserViewModel)
    {
        _logger = logger;
        _workshopClient = workshopClient;
        _workshopService = workshopService;
        _router = router;
        CurrentUserViewModel = currentUserViewModel;
        SearchAsync = ExecuteSearchAsync;

        this.WhenAnyValue(vm => vm.SelectedEntry).WhereNotNull().Subscribe(NavigateToEntry);
    }

    public CurrentUserViewModel CurrentUserViewModel { get; }

    public Func<string?, CancellationToken, Task<IEnumerable<object>>> SearchAsync { get; }

    public SearchResultViewModel? SelectedEntry
    {
        get => _selectedEntry;
        set => RaiseAndSetIfChanged(ref _selectedEntry, value);
    }

    public EntryType? EntryType
    {
        get => _entryType;
        set => RaiseAndSetIfChanged(ref _entryType, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => RaiseAndSetIfChanged(ref _isLoading, value);
    }

    private void NavigateToEntry(SearchResultViewModel searchResult)
    {
        string? url = null;
        if (searchResult.Entry.EntryType == WebClient.Workshop.EntryType.Profile)
            url = $"workshop/profiles/{searchResult.Entry.Id}";
        if (searchResult.Entry.EntryType == WebClient.Workshop.EntryType.Layout)
            url = $"workshop/layouts/{searchResult.Entry.Id}";

        if (url != null)
            Task.Run(() => _router.Navigate(url));
    }

    private async Task<IEnumerable<object>> ExecuteSearchAsync(string? input, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(input) || input.Length < 2)
                return new List<object>();

            IsLoading = true;
            IOperationResult<ISearchEntriesResult> results = await _workshopClient.SearchEntries.ExecuteAsync(input, EntryType, cancellationToken);
            return results.Data?.SearchEntries.Select(e => new SearchResultViewModel(e, _workshopService) as object) ?? new List<object>();
        }
        catch (Exception e)
        {
            if (e is not TaskCanceledException)
                _logger.Error(e, "Failed to execute search");
        }
        finally
        {
            IsLoading = false;
        }

        return new List<object>();
    }
}
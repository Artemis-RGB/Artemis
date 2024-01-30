using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.Screens.Root;
using Artemis.UI.Screens.Workshop.CurrentUser;
using Artemis.UI.Services.Interfaces;
using Artemis.UI.Shared;
using Artemis.WebClient.Workshop;
using Artemis.WebClient.Workshop.Services;
using PropertyChanged.SourceGenerator;
using ReactiveUI;
using Serilog;
using StrawberryShake;

namespace Artemis.UI.Screens.Workshop.Search;

public partial class SearchViewModel : ViewModelBase
{
    private readonly ILogger _logger;
    private readonly IWorkshopService _workshopService;
    private readonly IDebugService _debugService;
    private readonly IWorkshopClient _workshopClient;
    [Notify] private SearchResultViewModel? _selectedEntry;
    [Notify] private bool _isLoading;

    public SearchViewModel(ILogger logger, IWorkshopClient workshopClient, IWorkshopService workshopService, IDebugService debugService, DefaultTitleBarViewModel defaultTitleBarViewModel)
    {
        _logger = logger;
        _workshopClient = workshopClient;
        _workshopService = workshopService;
        _debugService = debugService;
        DefaultTitleBarViewModel = defaultTitleBarViewModel;
        SearchAsync = ExecuteSearchAsync;

        this.WhenAnyValue(vm => vm.SelectedEntry).WhereNotNull().Subscribe(NavigateToEntry);
    }

    public DefaultTitleBarViewModel DefaultTitleBarViewModel { get; }

    public Func<string?, CancellationToken, Task<IEnumerable<object>>> SearchAsync { get; }
    
    public void ShowDebugger()
    {
        _debugService.ShowDebugger();
    }
    
    private void NavigateToEntry(SearchResultViewModel searchResult)
    {
        _workshopService.NavigateToEntry(searchResult.Entry.Id, searchResult.Entry.EntryType);
    }

    private async Task<IEnumerable<object>> ExecuteSearchAsync(string? input, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(input) || input.Length < 2)
                return new List<object>();

            IsLoading = true;
            IOperationResult<ISearchEntriesResult> results = await _workshopClient.SearchEntries.ExecuteAsync(input, null, cancellationToken);
            return results.Data?.SearchEntries.Select(e => new SearchResultViewModel(e) as object) ?? new List<object>();
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
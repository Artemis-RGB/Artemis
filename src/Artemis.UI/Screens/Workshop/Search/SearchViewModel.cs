using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.Screens.Workshop.CurrentUser;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Routing;
using Artemis.WebClient.Workshop;
using ReactiveUI;
using StrawberryShake;

namespace Artemis.UI.Screens.Workshop.Search;

public class SearchViewModel : ViewModelBase
{
    public CurrentUserViewModel CurrentUserViewModel { get; }
    private readonly IRouter _router;
    private readonly IWorkshopClient _workshopClient;
    private EntryType? _entryType;
    private ISearchEntries_Entries_Nodes? _selectedEntry;

    public SearchViewModel(IWorkshopClient workshopClient, IRouter router, CurrentUserViewModel currentUserViewModel)
    {
        CurrentUserViewModel = currentUserViewModel;
        _workshopClient = workshopClient;
        _router = router;
        SearchAsync = ExecuteSearchAsync;

        this.WhenAnyValue(vm => vm.SelectedEntry).WhereNotNull().Subscribe(NavigateToEntry);
    }

    public Func<string?, CancellationToken, Task<IEnumerable<object>>> SearchAsync { get; }

    public ISearchEntries_Entries_Nodes? SelectedEntry
    {
        get => _selectedEntry;
        set => RaiseAndSetIfChanged(ref _selectedEntry, value);
    }

    public EntryType? EntryType
    {
        get => _entryType;
        set => RaiseAndSetIfChanged(ref _entryType, value);
    }

    private void NavigateToEntry(ISearchEntries_Entries_Nodes entry)
    {
        string? url = null;
        if (entry.EntryType == WebClient.Workshop.EntryType.Profile)
            url = $"workshop/profiles/{entry.Id}";
        if (entry.EntryType == WebClient.Workshop.EntryType.Layout)
            url = $"workshop/layouts/{entry.Id}";

        if (url != null)
            Task.Run(() => _router.Navigate(url));
    }

    private async Task<IEnumerable<object>> ExecuteSearchAsync(string? input, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(input))
            return new List<object>();

        EntryFilterInput filter;
        if (EntryType != null)
            filter = new EntryFilterInput
            {
                And = new[]
                {
                    new EntryFilterInput {EntryType = new EntryTypeOperationFilterInput {Eq = EntryType}},
                    new EntryFilterInput {Name = new StringOperationFilterInput {Contains = input}}
                }
            };
        else
            filter = new EntryFilterInput {Name = new StringOperationFilterInput {Contains = input}};

        IOperationResult<ISearchEntriesResult> results = await _workshopClient.SearchEntries.ExecuteAsync(filter, cancellationToken);
        return results.Data?.Entries?.Nodes?.Cast<object>() ?? new List<object>();
    }
}
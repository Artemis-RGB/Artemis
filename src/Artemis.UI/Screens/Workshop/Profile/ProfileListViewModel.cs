using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.Screens.Workshop.Categories;
using Artemis.UI.Screens.Workshop.Parameters;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Routing;
using Artemis.WebClient.Workshop;
using DynamicData;
using ReactiveUI;
using StrawberryShake;

namespace Artemis.UI.Screens.Workshop.Profile;

public class ProfileListViewModel : RoutableScreen<ActivatableViewModelBase, WorkshopListParameters>, IWorkshopViewModel
{
    private readonly SourceList<IGetEntries_Entries_Nodes> _entries;
    private readonly IWorkshopClient _workshopClient;
    private int _page;

    public ProfileListViewModel(IWorkshopClient workshopClient, CategoriesViewModel categoriesViewModel)
    {
        _workshopClient = workshopClient;
        CategoriesViewModel = categoriesViewModel;

        _entries = new SourceList<IGetEntries_Entries_Nodes>();
        _entries.Connect()
            .Transform(e => new ProfileListEntryViewModel(e))
            .Bind(out ReadOnlyObservableCollection<ProfileListEntryViewModel> observableEntries)
            .Subscribe();
        Entries = observableEntries;
    }

    public CategoriesViewModel CategoriesViewModel { get; }
    public ReadOnlyObservableCollection<ProfileListEntryViewModel> Entries { get; set; }

    public int Page
    {
        get => _page;
        set => RaiseAndSetIfChanged(ref _page, value);
    }

    public override async Task OnNavigating(WorkshopListParameters parameters, NavigationArguments args, CancellationToken cancellationToken)
    {
        Page = Math.Max(1, parameters.Page);
        await GetEntries(cancellationToken);
    }

    private async Task GetEntries(CancellationToken cancellationToken)
    {
        IOperationResult<IGetEntriesResult> result = await _workshopClient.GetEntries.ExecuteAsync(CreateFilter(), cancellationToken);
        if (result.IsErrorResult() || result.Data?.Entries?.Nodes == null)
            return;

        _entries.Edit(e =>
        {
            e.Clear();
            e.AddRange(result.Data.Entries.Nodes);
        });
    }

    private EntryFilterInput CreateFilter()
    {
        return new EntryFilterInput {EntryType = new EntryTypeOperationFilterInput {Eq = WebClient.Workshop.EntryType.Profile}};
    }

    public EntryType? EntryType => null;
}
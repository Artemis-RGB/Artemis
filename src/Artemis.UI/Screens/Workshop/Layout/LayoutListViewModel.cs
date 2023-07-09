using System;
using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.Screens.Workshop.Categories;
using Artemis.UI.Screens.Workshop.Parameters;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Routing;
using Artemis.WebClient.Workshop;

namespace Artemis.UI.Screens.Workshop.Layout;

public class LayoutListViewModel : RoutableScreen<ActivatableViewModelBase, WorkshopListParameters>, IWorkshopViewModel
{
    private int _page;

    public LayoutListViewModel(CategoriesViewModel categoriesViewModel)
    {
        CategoriesViewModel = categoriesViewModel;
    }

    public CategoriesViewModel CategoriesViewModel { get; }

    public int Page
    {
        get => _page;
        set => RaiseAndSetIfChanged(ref _page, value);
    }

    public override Task OnNavigating(WorkshopListParameters parameters, NavigationArguments args, CancellationToken cancellationToken)
    {
        Page = Math.Max(1, parameters.Page);
        return Task.CompletedTask;
    }

    public EntryType? EntryType => WebClient.Workshop.EntryType.Layout;
}
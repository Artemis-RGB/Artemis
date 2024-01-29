using System;
using Artemis.UI.Screens.Workshop.Categories;
using Artemis.UI.Screens.Workshop.Entries.List;
using Artemis.UI.Shared.Routing;
using Artemis.UI.Shared.Services;
using Artemis.WebClient.Workshop;

namespace Artemis.UI.Screens.Workshop.Entries.Tabs;

public class LayoutListViewModel : List.EntryListViewModel
{
    public LayoutListViewModel(IWorkshopClient workshopClient,
        IRouter router,
        CategoriesViewModel categoriesViewModel,
        EntryListInputViewModel entryListInputViewModel,
        INotificationService notificationService,
        Func<IGetEntries_Entries_Items, EntryListItemViewModel> getEntryListViewModel)
        : base("workshop/entries/layouts", workshopClient, router, categoriesViewModel, entryListInputViewModel, notificationService, getEntryListViewModel)
    {
        entryListInputViewModel.SearchWatermark = "Search layouts";
    }

    protected override EntryFilterInput GetFilter()
    {
        return new EntryFilterInput
        {
            And = new[]
            {
                new EntryFilterInput {EntryType = new EntryTypeOperationFilterInput {Eq = EntryType.Layout}},
                base.GetFilter()
            }
        };
    }
}
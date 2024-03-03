using System;
using Artemis.UI.Screens.Workshop.Categories;
using Artemis.UI.Screens.Workshop.Entries.List;
using Artemis.UI.Shared.Routing;
using Artemis.UI.Shared.Services;
using Artemis.WebClient.Workshop;

namespace Artemis.UI.Screens.Workshop.Entries.Tabs;

public class PluginListViewModel : EntryListViewModel
{
    public PluginListViewModel(IWorkshopClient workshopClient,
        IRouter router,
        CategoriesViewModel categoriesViewModel,
        EntryListInputViewModel entryListInputViewModel,
        INotificationService notificationService,
        Func<IEntrySummary, EntryListItemViewModel> getEntryListViewModel)
        : base("workshop/entries/plugins", workshopClient, router, categoriesViewModel, entryListInputViewModel, notificationService, getEntryListViewModel)
    {
        entryListInputViewModel.SearchWatermark = "Search plugins";
    }

    protected override EntryFilterInput GetFilter()
    {
        return new EntryFilterInput
        {
            And = new[]
            {
                new EntryFilterInput {EntryType = new EntryTypeOperationFilterInput {Eq = EntryType.Plugin}},
                base.GetFilter()
            }
        };
    }
}
using System;
using Artemis.UI.Screens.Workshop.Categories;
using Artemis.UI.Screens.Workshop.Entries;
using Artemis.UI.Shared.Routing;
using Artemis.UI.Shared.Services;
using Artemis.WebClient.Workshop;

namespace Artemis.UI.Screens.Workshop.Profile;

public class ProfileListViewModel : EntryListBaseViewModel
{
    /// <inheritdoc />
    public ProfileListViewModel(IWorkshopClient workshopClient,
        IRouter router,
        CategoriesViewModel categoriesViewModel,
        INotificationService notificationService,
        Func<IGetEntries_Entries_Items, EntryListItemViewModel> getEntryListViewModel)
        : base(workshopClient, router, categoriesViewModel, notificationService, getEntryListViewModel)
    {
    }

    #region Overrides of EntryListBaseViewModel

    /// <inheritdoc />
    protected override string GetPagePath(int page)
    {
        return $"workshop/profiles/{page}";
    }

    /// <inheritdoc />
    protected override EntryFilterInput GetFilter()
    {
        return new EntryFilterInput
        {
            And = new[]
            {
                new EntryFilterInput {EntryType = new EntryTypeOperationFilterInput {Eq = WebClient.Workshop.EntryType.Profile}},
                base.GetFilter()
            }
        };
    }

    #endregion
}
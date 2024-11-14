using Artemis.UI.Shared.Routing;
using Artemis.WebClient.Workshop;
using Artemis.WebClient.Workshop.Services;

namespace Artemis.UI.Screens.Workshop.Entries.List;

public class EntryListItemVerticalViewModel : EntryListItemViewModel
{
    /// <inheritdoc />
    public EntryListItemVerticalViewModel(IEntrySummary entry, IRouter router, IWorkshopService workshopService) : base(entry, router, workshopService)
    {
    }
}
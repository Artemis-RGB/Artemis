using System;
using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.Screens.Workshop.Entries;
using Artemis.UI.Screens.Workshop.Parameters;
using Artemis.UI.Shared.Routing;

namespace Artemis.UI.Screens.Workshop.Library;

public class SubmissionsDetailViewModel : RoutableScreen<WorkshopDetailParameters>
{
    public EntrySpecificationsViewModel EntrySpecificationsViewModel { get; }

    public SubmissionsDetailViewModel(EntrySpecificationsViewModel entrySpecificationsViewModel)
    {
        EntrySpecificationsViewModel = entrySpecificationsViewModel;
    }

    public override Task OnNavigating(WorkshopDetailParameters parameters, NavigationArguments args, CancellationToken cancellationToken)
    {
        Console.WriteLine(parameters.EntryId);
        return Task.CompletedTask;
    }
}
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Routing;
using Artemis.WebClient.Workshop;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.Library.Tabs;

public class SubmissionsTabItemViewModel : ViewModelBase
{
    private readonly IRouter _router;

    public SubmissionsTabItemViewModel(IGetSubmittedEntries_SubmittedEntries entry, IRouter router)
    {
        _router = router;
        Entry = entry;

        NavigateToEntry = ReactiveCommand.CreateFromTask(ExecuteNavigateToEntry);
    }

    public IGetSubmittedEntries_SubmittedEntries Entry { get; }
    public ReactiveCommand<Unit, Unit> NavigateToEntry { get; }

    private async Task ExecuteNavigateToEntry(CancellationToken cancellationToken)
    {
        await _router.Navigate($"workshop/library/submissions/{Entry.Id}");
    }
}
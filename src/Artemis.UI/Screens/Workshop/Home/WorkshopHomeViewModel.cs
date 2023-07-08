using System.Reactive;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Routing;
using Artemis.WebClient.Workshop;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.Home;

public class WorkshopHomeViewModel : ActivatableViewModelBase, IWorkshopViewModel
{
    public WorkshopHomeViewModel(IRouter router)
    {
        Navigate = ReactiveCommand.CreateFromTask<string>(async r => await router.Navigate(r));
    }

    public ReactiveCommand<string, Unit> Navigate { get; set; }

    public EntryType? EntryType => null;

}
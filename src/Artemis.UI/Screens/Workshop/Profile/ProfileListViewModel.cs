using System;
using System.Threading;
using System.Threading.Tasks;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Routing;

namespace Artemis.UI.Screens.Workshop.Profile;

public class ProfileListViewModel : RoutableScreen<ActivatableViewModelBase, WorkshopListParameters>, IMainScreenViewModel
{
    private int _page;

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
    
    public ViewModelBase? TitleBarViewModel => null;
}
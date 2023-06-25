using System.Threading.Tasks;
using DryIoc;

namespace Artemis.UI.Shared.Routing;

public abstract class ActivatableRoutable : ActivatableViewModelBase, IRoutable
{
    private ViewModelBase _currentScreen;

    public ViewModelBase CurrentScreen
    {
        get => _currentScreen;
        private set => RaiseAndSetIfChanged(ref _currentScreen, value);
    }

    /// <inheritdoc />
    public async Task<bool> Activate(RouteResolution routeResolution, IContainer container)
    {
        ViewModelBase viewModel = routeResolution.GetViewModel<ViewModelBase>(container);
        if (routeResolution.Child != null)
        {
            if (viewModel is not IRoutable routableScreen)
                throw new ArtemisRoutingException($"Route resolved with a child but view model of type {routeResolution.ViewModel} is does mot implement {nameof(IRoutable)}.");

            CurrentScreen = viewModel;
            return await routableScreen.Activate(routeResolution.Child, container);
        }

        CurrentScreen = viewModel;
        return true;
    }
}
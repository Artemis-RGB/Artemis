using System.Threading.Tasks;
using DryIoc;

namespace Artemis.UI.Shared.Routing;

public abstract class Routable : ViewModelBase, IRoutable
{
    public ViewModelBase CurrentScreen { get; private set; }

    /// <inheritdoc />
    public virtual async Task<bool> Activate(RouteResolution routeResolution, IContainer container)
    {
        // Reuse the view model if it has not changed, this might not always be a good idea though
        ViewModelBase viewModel = routeResolution.ViewModel == CurrentScreen.GetType() 
            ? CurrentScreen 
            : routeResolution.GetViewModel<ViewModelBase>(container);
        
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
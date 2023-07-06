using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Artemis.UI.Shared.Routing;

public class RouteRegistration<TViewModel> : IRouterRegistration where TViewModel : ViewModelBase
{
    public RouteRegistration(string path)
    {
        Route = new Route(path);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{nameof(Route)}: {Route}, {nameof(ViewModel)}: {ViewModel}";
    }

    public Route Route { get; }

    /// <inheritdoc />
    public Type ViewModel => typeof(TViewModel);

    /// <inheritdoc />
    public List<IRouterRegistration> Children { get; set; } = new();
}

public interface IRouterRegistration
{
    Route Route { get; }
    Type ViewModel { get; }
    List<IRouterRegistration> Children { get; set; }

}
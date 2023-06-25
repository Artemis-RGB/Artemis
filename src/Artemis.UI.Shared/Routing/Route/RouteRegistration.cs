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

    public Route Route { get; }

    /// <inheritdoc />
    public Type ViewModel => typeof(TViewModel);

    /// <inheritdoc />
    public List<IRouterRegistration> Children { get; set; } = new();

    /// <inheritdoc />
    public async Task<RouteResolution> Resolve(string path)
    {
        List<string> segments = path.Split('/').ToList();
        if (Route.Segments.Count > segments.Count)
            return RouteResolution.AsFailure();

        // Ensure self is a match
        List<object> parameters = new();
        int currentSegment = 0;
        foreach (RouteSegment routeSegment in Route.Segments)
        {
            string segment = segments[currentSegment];
            if (!routeSegment.IsMatch(segment))
                return RouteResolution.AsFailure();

            object? parameter = routeSegment.GetParameter(segment);
            if (parameter != null)
                parameters.Add(parameter);

            currentSegment++;
        }

        if (currentSegment == segments.Count)
            return RouteResolution.AsSuccess(ViewModel, parameters.ToArray());

        // If segments remain, a child should match it
        path = string.Join('/', segments);
        foreach (IRouterRegistration routerRegistration in Children)
        {
            RouteResolution result = await routerRegistration.Resolve(path);
            if (result.Success)
                return RouteResolution.AsSuccess(ViewModel, parameters.ToArray(), result);
        }

        return RouteResolution.AsFailure();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{nameof(Route)}: {Route}, {nameof(ViewModel)}: {ViewModel}";
    }
}

public interface IRouterRegistration
{
    Route Route { get; }
    Type ViewModel { get; }
    List<IRouterRegistration> Children { get; set; }

    Task<RouteResolution> Resolve(string path);
}
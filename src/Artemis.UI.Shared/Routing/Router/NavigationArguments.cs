using System;
using System.Threading.Tasks;

namespace Artemis.UI.Shared.Routing;

/// <summary>
/// Represents an object that contains information about the current navigation action.
/// </summary>
public class NavigationArguments
{
    internal NavigationArguments(IRouter router, string path, object[] routeParameters)
    {
        Router = router;
        Path = path;
        RouteParameters = routeParameters;
        SegmentParameters = Array.Empty<object>();
    }

    /// <summary>
    /// Gets the router in which the navigation is taking place.
    /// </summary>
    public IRouter Router { get; }

    /// <summary>
    /// Gets the path of the route that is being navigated to.
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// GEts an array of all parameters provided to this route.
    /// </summary>
    public object[] RouteParameters { get; }

    /// <summary>
    /// Gets an array of parameters provided to this screen's segment of the route.
    /// </summary>
    public object[] SegmentParameters { get; internal set; }

    internal bool Cancelled { get; private set; }

    /// <summary>
    /// Cancels further processing of the current navigation. 
    /// </summary>
    /// <remarks>It not necessary to cancel the navigation in order to navigate to another route, the current navigation will be cancelled by the router.</remarks>
    public void Cancel()
    {
        Cancelled = true;
    }
}
using System;
using System.Collections.Generic;

namespace Artemis.UI.Shared.Routing;

/// <summary>
///     Represents a registration for a route and its associated view model.
/// </summary>
/// <typeparam name="TViewModel">The type of the view model associated with the route.</typeparam>
public class RouteRegistration<TViewModel> : IRouterRegistration where TViewModel : RoutableScreen
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RouteRegistration{TViewModel}" /> class.
    /// </summary>
    /// <param name="path">The path of the route.</param>
    public RouteRegistration(string path)
    {
        Route = new Route(path);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{nameof(Route)}: {Route}, {nameof(ViewModel)}: {ViewModel}";
    }

    /// <summary>
    ///     Gets the route associated with this registration.
    /// </summary>
    public Route Route { get; }

    /// <inheritdoc />
    public Type ViewModel => typeof(TViewModel);

    /// <inheritdoc />
    public List<IRouterRegistration> Children { get; set; } = new();
}
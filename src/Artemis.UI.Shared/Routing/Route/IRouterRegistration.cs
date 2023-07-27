using System;
using System.Collections.Generic;

namespace Artemis.UI.Shared.Routing;

/// <summary>
///     Represents a registration for a route.
/// </summary>
public interface IRouterRegistration
{
    /// <summary>
    ///     Gets the route associated with this registration.
    /// </summary>
    Route Route { get; }

    /// <summary>
    ///     Gets the type of the view model associated with the route.
    /// </summary>
    Type ViewModel { get; }

    /// <summary>
    ///     Gets or sets the child registrations of this route.
    /// </summary>
    List<IRouterRegistration> Children { get; set; }
}
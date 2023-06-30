using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Artemis.UI.Shared.Routing;

/// <summary>
///     Represents a router that can be used to navigate to different screens.
/// </summary>
public interface IRouter
{
    /// <summary>
    ///     Gets or sets the IRoutable that hosts the root of the navigation.
    /// </summary>
    IRoutable? Root { get; set; }

    /// <summary>
    ///     Gets an observable containing the current path.
    /// </summary>
    IObservable<string?> CurrentPath { get; }

    /// <summary>
    ///     Gets a list of router registrations, you can use this to register new routes.
    /// </summary>
    List<IRouterRegistration> Routes { get; }

    /// <summary>
    ///     Asynchronously navigates to the provided path.
    /// </summary>
    /// <remarks>Navigating cancels any currently processing navigations.</remarks>
    /// <param name="path">The path to navigate to.</param>
    /// <param name="options">Optional navigation options used to control navigation behaviour.</param>
    /// <returns>A task representing the operation</returns>
    Task Navigate(string path, RouterNavigationOptions? options = null);

    /// <summary>
    ///     Asynchronously navigates back to the previous active route.
    /// </summary>
    /// <returns>A task containing a boolean value which indicates whether there was a previous path to go back to.</returns>
    Task<bool> GoBack();

    /// <summary>
    ///     Asynchronously navigates forward to the previous active route.
    /// </summary>
    /// <returns>A task containing a boolean value which indicates whether there was a forward path to go back to.</returns>
    Task<bool> GoForward();

    /// <summary>
    ///     Clears the navigation history.
    /// </summary>
    void ClearHistory();
}
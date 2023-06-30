using System;
using System.Threading;
using System.Threading.Tasks;

namespace Artemis.UI.Shared.Routing;

/// <summary>
/// Represents a screen that can be navigated to.
/// </summary>
public interface INavigable
{
    /// <summary>
    /// Called when this screen is being navigated to.
    /// </summary>
    /// <param name="args">Navigation arguments containing information about the navigation action.</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    Task Navigated(NavigationArguments args, CancellationToken cancellationToken);
}

/// <summary>
/// Represents a screen that can be navigated to.
/// </summary>
/// <typeparam name="TParam">A class containing a property with in matching order and type for each parameter of the route.</typeparam>
public interface INavigable<in TParam> : INavigable where TParam : class
{
    /// <summary>
    /// Called when this screen is being navigated to.
    /// </summary>
    /// <param name="parameters">An object containing the parameters of the navigation action.</param>
    /// <param name="args">Navigation arguments containing information about the navigation action.</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    Task Navigated(TParam parameters, NavigationArguments args, CancellationToken cancellationToken);

    /// <inheritdoc />
    async Task INavigable.Navigated(NavigationArguments args, CancellationToken cancellationToken)
    {
        if (args.SegmentParameters.Length == 0)
            throw new ArtemisRoutingException("Cannot navigate to this INavigable without parameters");

        TParam? parameters = (TParam?) Activator.CreateInstance(typeof(TParam), args.SegmentParameters);
        if (parameters == null)
            throw new ArtemisRoutingException("Failed to transform parameters into object instance");

        await Navigated(parameters, args, cancellationToken);
    }
}
using System.Threading;
using System.Threading.Tasks;

namespace Artemis.UI.Shared.Routing;

/// <summary>
///     Represents a view model to which routing can take place.
/// </summary>
public abstract class RoutableScreen : ActivatableViewModelBase, IRoutableScreen
{
    /// <summary>
    ///     Called before navigating to this screen.
    /// </summary>
    /// <param name="args">Navigation arguments containing information about the navigation action.</param>
    public virtual Task BeforeNavigating(NavigationArguments args)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Called while navigating to this screen.
    /// </summary>
    /// <param name="args">Navigation arguments containing information about the navigation action.</param>
    /// <param name="cancellationToken">
    ///     A cancellation token that can be used by other objects or threads to receive notice of
    ///     cancellation.
    /// </param>
    public virtual Task OnNavigating(NavigationArguments args, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Called before navigating away from this screen.
    /// </summary>
    /// <param name="args">Navigation arguments containing information about the navigation action.</param>
    public virtual Task OnClosing(NavigationArguments args)
    {
        return Task.CompletedTask;
    }

    #region Overrides of RoutableScreen

    async Task IRoutableScreen.InternalOnNavigating(NavigationArguments args, CancellationToken cancellationToken)
    {
        await OnNavigating(args, cancellationToken);
    }

    async Task IRoutableScreen.InternalOnClosing(NavigationArguments args)
    {
        await OnClosing(args);
    }

    #endregion
}
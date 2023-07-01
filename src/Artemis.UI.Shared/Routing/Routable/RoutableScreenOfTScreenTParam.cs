using System.Threading;
using System.Threading.Tasks;

namespace Artemis.UI.Shared.Routing;

/// <summary>
/// Represents a view model to which routing with parameters can take place and which in turn can host another view model.
/// </summary>
/// <typeparam name="TScreen">The type of view model the screen can host.</typeparam>
/// <typeparam name="TParam">The type of parameters the screen expects.</typeparam>
public abstract class RoutableScreen<TScreen, TParam> : ActivatableViewModelBase, IRoutableScreen where TScreen : class
{
    private TScreen? _screen;
    private bool _recycleScreen = true;

    /// <summary>
    /// Gets the currently active child screen.
    /// </summary>
    public TScreen? Screen
    {
        get => _screen;
        private set => RaiseAndSetIfChanged(ref _screen, value);
    }

    /// <inheritdoc />
    public bool RecycleScreen
    {
        get => _recycleScreen;
        protected set => RaiseAndSetIfChanged(ref _recycleScreen, value);
    }

    /// <summary>
    /// Called before navigating to this screen.
    /// </summary>
    /// <param name="args">Navigation arguments containing information about the navigation action.</param>
    public virtual Task BeforeNavigating(NavigationArguments args)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Called while navigating to this screen.
    /// </summary>
    /// <param name="parameters">An object containing the parameters of the navigation action.</param>
    /// <param name="args">Navigation arguments containing information about the navigation action.</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    public virtual Task OnNavigating(TParam parameters, NavigationArguments args, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Called before navigating away from this screen.
    /// </summary>
    /// <param name="args">Navigation arguments containing information about the navigation action.</param>
    public virtual Task OnClosing(NavigationArguments args)
    {
        return Task.CompletedTask;
    }

    #region Overrides of RoutableScreen

    object? IRoutableScreen.InternalScreen => Screen;

    void IRoutableScreen.InternalChangeScreen(object? screen)
    {
        Screen = screen as TScreen;
    }

    async Task IRoutableScreen.InternalOnNavigating(NavigationArguments args, CancellationToken cancellationToken)
    {
        if (args.SegmentParameters.Length == 0)
            throw new ArtemisRoutingException("Cannot navigate to this RoutableViewModel without parameters");

        TParam? parameters = (TParam?) System.Activator.CreateInstance(typeof(TParam), args.SegmentParameters);
        if (parameters == null)
            throw new ArtemisRoutingException("Failed to transform parameters into object instance");

        await OnNavigating(parameters, args, cancellationToken);
    }

    async Task IRoutableScreen.InternalOnClosing(NavigationArguments args)
    {
        await OnClosing(args);
    }

    #endregion
}
using System.Threading;
using System.Threading.Tasks;

namespace Artemis.UI.Shared.Routing;

/// <summary>
/// Represents a view model to which routing can take place and which in turn can host another view model.
/// </summary>
/// <typeparam name="TScreen">The type of view model the screen can host.</typeparam>
public abstract class RoutableScreen<TScreen> : ActivatableViewModelBase, IRoutableScreen where TScreen : class
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
    /// <param name="args">Navigation arguments containing information about the navigation action.</param>
    /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
    public virtual Task OnNavigating(NavigationArguments args, CancellationToken cancellationToken)
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
        await OnNavigating(args, cancellationToken);
    }

    async Task IRoutableScreen.InternalOnClosing(NavigationArguments args)
    {
        await OnClosing(args);
    }
    
    #endregion
}
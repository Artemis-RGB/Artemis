namespace Artemis.UI.Shared.Routing;

/// <summary>
///     Represents a view model to which routing can take place and which in turn can host another view model.
/// </summary>
/// <typeparam name="TScreen">The type of view model the screen can host.</typeparam>
public abstract class RoutableHostScreen<TScreen> : RoutableScreen, IRoutableHostScreen where TScreen : RoutableScreen
{
    private bool _recycleScreen = true;
    private TScreen? _screen;

    /// <summary>
    ///     Gets the currently active child screen.
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

    IRoutableScreen? IRoutableHostScreen.InternalScreen => Screen;

    void IRoutableHostScreen.InternalChangeScreen(IRoutableScreen? screen)
    {
        if (screen == null)
        {
            Screen = null;
            return;
        }

        if (screen is not TScreen typedScreen)
            throw new ArtemisRoutingException($"Screen cannot be hosted, {screen.GetType().Name} is not assignable to {typeof(TScreen).Name}.");
        Screen = typedScreen;
    }
}
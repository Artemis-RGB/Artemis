using ReactiveUI;

namespace Artemis.UI.Shared.Routing;

public abstract class Routable : ViewModelBase, IRoutable
{
    private ViewModelBase? _currentScreen;

    /// <summary>
    ///     Gets the current screen.
    /// </summary>
    public ViewModelBase? CurrentScreen
    {
        get => _currentScreen;
        private set => RaiseAndSetIfChanged(ref _currentScreen, value);
    }

    /// <inheritdoc />
    public object? Screen => CurrentScreen;

    /// <inheritdoc />
    public bool RecycleScreen { get; protected set; } = true;

    /// <inheritdoc />
    public void ChangeScreen(object screen)
    {
        CurrentScreen = screen as ViewModelBase;
        this.RaisePropertyChanged(nameof(Screen));
    }
}
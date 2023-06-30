namespace Artemis.UI.Shared.Routing;

/// <summary>
/// Represents a screen that can be navigated to.
/// </summary>
public interface IRoutable
{
    object? Screen { get; }
    bool RecycleScreen { get; }
    void ChangeScreen(object screen);
}
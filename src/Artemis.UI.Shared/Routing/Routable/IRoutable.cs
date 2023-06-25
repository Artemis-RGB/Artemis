namespace Artemis.UI.Shared.Routing;

public interface IRoutable
{
    object? Screen { get; }
    void ChangeScreen(object screen);
}
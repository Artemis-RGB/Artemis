using ReactiveUI;

namespace Artemis.UI.Shared.Routing;

public abstract class ActivatableRoutable : Routable, IActivatableViewModel
{
    /// <inheritdoc />
    public ViewModelActivator Activator { get; } = new();
}
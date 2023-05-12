using System.Reflection;
using Artemis.UI.Shared.Services;
using DryIoc;

namespace Artemis.UI.Shared.DryIoc;

/// <summary>
/// Provides an extension method to register services onto a DryIoc <see cref="IContainer"/>.
/// </summary>
public static class ContainerExtensions
{
    /// <summary>
    /// Registers shared UI services into the container.
    /// </summary>
    /// <param name="container">The builder building the current container</param>
    public static void RegisterSharedUI(this IContainer container)
    {
        Assembly artemisShared = typeof(IArtemisSharedUIService).GetAssembly();
        container.RegisterMany(new[] {artemisShared}, type => type.IsAssignableTo<IArtemisSharedUIService>(), Reuse.Singleton);

        UI.Locator = container;
    }
}
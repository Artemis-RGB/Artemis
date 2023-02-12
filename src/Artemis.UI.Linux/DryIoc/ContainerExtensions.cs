using Artemis.Core.Services;
using Artemis.UI.Linux.Providers.Input;
using DryIoc;

namespace Artemis.UI.Linux.DryIoc;

/// <summary>
/// Provides an extension method to register services onto a DryIoc <see cref="IContainer"/>.
/// </summary>
public static class UIContainerExtensions
{
    /// <summary>
    /// Registers providers into the container.
    /// </summary>
    /// <param name="container">The builder building the current container</param>
    public static void RegisterProviders(this IContainer container)
    {
        container.Register<InputProvider, LinuxInputProvider>(serviceKey: LinuxInputProvider.Id);
    }
}
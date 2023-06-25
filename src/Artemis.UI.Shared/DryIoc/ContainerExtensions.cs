using System.Reflection;
using Artemis.UI.Shared.Routing;
using Artemis.UI.Shared.Services;
using DryIoc;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
        container.Register<IRouter, Router>(Reuse.Singleton);
        Assembly artemisShared = typeof(IArtemisSharedUIService).GetAssembly();
        container.RegisterMany(new[] {artemisShared}, type => type.IsAssignableTo<IArtemisSharedUIService>(), Reuse.Singleton);

        UI.Locator = container;
    }
}
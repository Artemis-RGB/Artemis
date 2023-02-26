using Artemis.Core.Providers;
using Artemis.Core.Services;
using Artemis.UI.Services.Updating;
using Artemis.UI.Shared.Providers;
using Artemis.UI.Windows.Providers;
using Artemis.UI.Windows.Providers.Input;
using DryIoc;

namespace Artemis.UI.Windows.DryIoc;

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
        container.Register<ICursorProvider, CursorProvider>(Reuse.Singleton);
        container.Register<IGraphicsContextProvider, GraphicsContextProvider>(Reuse.Singleton);
        container.Register<IAutoRunProvider, AutoRunProvider>();
        container.Register<InputProvider, WindowsInputProvider>(serviceKey: WindowsInputProvider.Id);
        container.Register<IUpdateNotificationProvider, WindowsUpdateNotificationProvider>();
    }
}
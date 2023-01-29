using Artemis.Core.DryIoc;
using Artemis.Core.Providers;
using Artemis.Core.Services;
using Artemis.UI.Shared.Providers;
using Artemis.UI.Windows.Providers;
using Artemis.UI.Windows.Providers.Input;
using DryIoc;

namespace Artemis.UI.Windows.DryIoc;

public class WindowsModule : IModule
{
    /// <inheritdoc />
    public void Load(IRegistrator builder)
    {
        builder.Register<ICursorProvider, CursorProvider>(Reuse.Singleton);
        builder.Register<IGraphicsContextProvider, GraphicsContextProvider>(Reuse.Singleton);
        builder.Register<IUpdateProvider, UpdateProvider>(Reuse.Singleton);
        builder.Register<IAutoRunProvider, AutoRunProvider>();
        builder.Register<InputProvider, WindowsInputProvider>(serviceKey: WindowsInputProvider.Id);
    }
}
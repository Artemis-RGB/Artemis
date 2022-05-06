using Artemis.Core.Services;
using Artemis.UI.Shared.Providers;
using Artemis.UI.Windows.Providers;
using Ninject.Modules;

namespace Artemis.UI.Windows.Ninject;

public class WindowsModule : NinjectModule
{
    #region Overrides of NinjectModule

    /// <inheritdoc />
    public override void Load()
    {
        Kernel!.Bind<ICursorProvider>().To<CursorProvider>().InSingletonScope();
        Kernel!.Bind<IGraphicsContextProvider>().To<GraphicsContextProvider>().InSingletonScope();
    }

    #endregion
}
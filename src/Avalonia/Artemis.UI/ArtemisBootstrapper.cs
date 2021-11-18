using Artemis.Core.Ninject;
using Artemis.UI.Exceptions;
using Artemis.UI.Ninject;
using Artemis.UI.Screens.Root.ViewModels;
using Artemis.UI.Shared.Ninject;
using Avalonia.Controls.ApplicationLifetimes;
using Ninject;
using Splat.Ninject;

namespace Artemis.UI
{
    public static class ArtemisBootstrapper
    {
        private static StandardKernel? _kernel;

        public static void Bootstrap()
        {
            if (_kernel != null)
                throw new ArtemisUIException("UI already bootstrapped");

            _kernel = new StandardKernel();
            _kernel.Settings.InjectNonPublic = true;

            _kernel.Load<CoreModule>();
            _kernel.Load<UIModule>();
            _kernel.Load<SharedUIModule>();

            _kernel.UseNinjectDependencyResolver();
        }

        public static void ConfigureApplicationLifetime(IClassicDesktopStyleApplicationLifetime applicationLifetime)
        {
            if (_kernel == null)
                throw new ArtemisUIException("UI not yet bootstrapped");

            applicationLifetime.MainWindow = new MainWindow {DataContext = _kernel.Get<RootViewModel>()};
        }
    }
}
using Artemis.Core.Ninject;
using Artemis.UI.Exceptions;
using Artemis.UI.Ninject;
using Artemis.UI.Screens.Root;
using Artemis.UI.Shared.Ninject;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Ninject;
using Splat.Ninject;

namespace Artemis.UI
{
    public static class ArtemisBootstrapper
    {
        private static StandardKernel? _kernel;
        private static Application? _application;

        public static StandardKernel Bootstrap(Application application)
        {
            if (_application != null || _kernel != null)
                throw new ArtemisUIException("UI already bootstrapped");
           
            _application = application;
            _kernel = new StandardKernel();
            _kernel.Settings.InjectNonPublic = true;

            _kernel.Load<CoreModule>();
            _kernel.Load<UIModule>();
            _kernel.Load<SharedUIModule>();

            _kernel.UseNinjectDependencyResolver();

            return _kernel;
        }

        public static void Initialize()
        {
            if (_application == null || _kernel == null)
                throw new ArtemisUIException("UI not yet bootstrapped");
            if (_application.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
                return; 

            // Don't shut down when the last window closes, we might still be active in the tray
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            // Create the root view model that drives the UI
            RootViewModel rootViewModel = _kernel.Get<RootViewModel>();
            // Apply the root view model to the data context of the application so that tray icon commands work
            _application.DataContext = rootViewModel;
        }
    }
}
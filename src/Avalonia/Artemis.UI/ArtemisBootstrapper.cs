using System;
using System.Reactive;
using Artemis.Core.Ninject;
using Artemis.Core;
using Artemis.UI.Exceptions;
using Artemis.UI.Ninject;
using Artemis.UI.Screens.Root;
using Artemis.UI.Shared.Ninject;
using Artemis.UI.Shared.Services.Interfaces;
using Artemis.VisualScripting.Ninject;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Ninject;
using Ninject.Modules;
using ReactiveUI;
using Splat.Ninject;

namespace Artemis.UI
{
    public static class ArtemisBootstrapper
    {
        private static StandardKernel? _kernel;
        private static Application? _application;

        public static StandardKernel Bootstrap(Application application, params INinjectModule[] modules)
        {
            if (_application != null || _kernel != null)
                throw new ArtemisUIException("UI already bootstrapped");

            Utilities.PrepareFirstLaunch();

            _application = application;
            _kernel = new StandardKernel();
            _kernel.Settings.InjectNonPublic = true;

            _kernel.Load<CoreModule>();
            _kernel.Load<UIModule>();
            _kernel.Load<SharedUIModule>();
            _kernel.Load<NoStringNinjectModule>();
            _kernel.Load(modules);

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

            RxApp.DefaultExceptionHandler = Observer.Create<Exception>(DisplayUnhandledException);
        }

        private static void DisplayUnhandledException(Exception exception)
        {
            _kernel?.Get<IWindowService>().ShowExceptionDialog("Exception", exception);
        }
    }
}
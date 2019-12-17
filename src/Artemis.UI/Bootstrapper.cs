using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Artemis.Core.Ninject;
using Artemis.Core.Services.Interfaces;
using Artemis.UI.Ninject;
using Artemis.UI.Screens;
using Artemis.UI.Screens.Splash;
using Artemis.UI.Stylet;
using Ninject;
using Serilog;
using Stylet;

namespace Artemis.UI
{
    public class Bootstrapper : NinjectBootstrapper<RootViewModel>
    {
        private ICoreService _core;

        protected override void OnExit(ExitEventArgs e)
        {
            // Stop the Artemis core
            _core?.Dispose();

            base.OnExit(e);
        }

        protected override void Launch()
        {
            var windowManager = (IWindowManager) GetInstance(typeof(IWindowManager));
            var splashViewModel = new SplashViewModel(Kernel);
            windowManager.ShowWindow(splashViewModel);

            Task.Run(() =>
            {
                try
                {
                    // Start the Artemis core
                    _core = Kernel.Get<ICoreService>();
                    // When the core is done, hide the splash and show the main window
                    _core.Initialized += (sender, args) => ShowMainWindow(windowManager, splashViewModel);
                    // While the core is instantiated, start listening for events on the splash
                    splashViewModel.ListenToEvents();
                }
                catch (Exception e)
                {
                    var logger = Kernel.Get<ILogger>();
                    logger.Fatal(e, "Fatal exception during initialization, shutting down.");

                    // TODO: A proper exception viewer
                    Execute.OnUIThread(() =>
                    {
                        windowManager.ShowMessageBox(e.Message + "\n\n Please refer the log file for more details.",
                            "Fatal exception during initialization",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error
                        );
                        Environment.Exit(1);
                    });

                    throw;
                }
            });
        }

        private void ShowMainWindow(IWindowManager windowManager, SplashViewModel splashViewModel)
        {
            Execute.OnUIThread(() =>
            {
                windowManager.ShowWindow(RootViewModel);
                splashViewModel.RequestClose();
            });
        }

        protected override void ConfigureIoC(IKernel kernel)
        {
            kernel.Settings.InjectNonPublic = true;

            // Load this assembly's module
            kernel.Load<UIModule>();
            // Load the core assembly's module
            kernel.Load<CoreModule>();
            base.ConfigureIoC(kernel);
        }

        protected override void OnUnhandledException(DispatcherUnhandledExceptionEventArgs e)
        {
            var logger = Kernel.Get<ILogger>();
            logger.Fatal(e.Exception, "Fatal exception, shutting down.");

            base.OnUnhandledException(e);
        }
    }
}
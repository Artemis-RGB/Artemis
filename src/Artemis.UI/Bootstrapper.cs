using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
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
    public class Bootstrapper : NinjectBootstrapper<TrayViewModel>
    {
        private ICoreService _core;
        public static List<string> StartupArguments { get; private set; }

        protected override void OnExit(ExitEventArgs e)
        {
            // Stop the Artemis core
            _core?.Dispose();

            base.OnExit(e);
        }

        protected override void Launch()
        {
            StartupArguments = Args.ToList();

            var windowManager = Kernel.Get<IWindowManager>();
            windowManager.ShowWindow(RootViewModel);

            Task.Run(() =>
            {
                try
                {
                    // Start the Artemis core
                    _core = Kernel.Get<ICoreService>();
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

        private void ShowMainWindow(IWindowManager windowManager, SplashViewModel splashViewModel)
        {
            Execute.OnUIThread(() =>
            {
                windowManager.ShowWindow(RootViewModel);
                splashViewModel.RequestClose();
            });
        }
    }
}
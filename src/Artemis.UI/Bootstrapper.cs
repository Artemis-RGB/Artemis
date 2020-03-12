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
using Artemis.UI.Shared.Services.Interfaces;
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

            var logger = Kernel.Get<ILogger>();
            var viewManager = Kernel.Get<IViewManager>();

            // Create the Artemis core
            try
            {
                _core = Kernel.Get<ICoreService>();
            }
            catch (Exception e)
            {
                HandleFatalException(e, logger);
                throw;
            }

            // Create and bind the root view, this is a tray icon so don't show it with the window manager
            Execute.OnUIThread(() => viewManager.CreateAndBindViewForModelIfNecessary(RootViewModel));

            // Initialize the core async so the UI can show the progress
            Task.Run(async () =>
            {
                try
                {
                    if (StartupArguments.Contains("-autorun"))
                    {
                        logger.Information("Sleeping for 15 seconds on auto run to allow applications like iCUE and LGS to start");
                        await Task.Delay(TimeSpan.FromSeconds(15));
                    }

                    _core.Initialize();
                }
                catch (Exception e)
                {
                    HandleFatalException(e, logger);
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
            logger.Fatal(e.Exception, "Unhandled exception");

            var dialogService = Kernel.Get<IDialogService>();
            try
            {
                dialogService.ShowExceptionDialog("Artemis encountered an error", e.Exception);
            }
            catch (Exception)
            {
                // We did our best eh.. trap exceptions during exception display here to avoid an infinite loop
            }

            // Don't shut down, is that a good idea? Depends on the exception of course..
            e.Handled = true;
        }

        private void HandleFatalException(Exception e, ILogger logger)
        {
            logger.Fatal(e, "Fatal exception during initialization, shutting down.");

            // Can't use a pretty exception dialog here since the UI might not even be visible
            Execute.OnUIThread(() =>
            {
                Kernel.Get<IWindowManager>().ShowMessageBox(e.Message + "\n\n Please refer the log file for more details.",
                    "Fatal exception during initialization",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                Environment.Exit(1);
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
    }
}
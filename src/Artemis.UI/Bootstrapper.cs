using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Threading;
using Artemis.Core.Ninject;
using Artemis.Core.Services;
using Artemis.UI.Ninject;
using Artemis.UI.Screens;
using Artemis.UI.Services;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Artemis.UI.Stylet;
using Ninject;
using Serilog;
using SharpVectors.Dom.Resources;
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
            Core.Utilities.ShutdownRequested += UtilitiesOnShutdownRequested;
            Core.Utilities.PrepareFirstLaunch();

            ILogger logger = Kernel.Get<ILogger>();
            IViewManager viewManager = Kernel.Get<IViewManager>();

            StartupArguments = Args.ToList();

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

            // Ensure WPF uses the right culture in binding converters
            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

            // Create and bind the root view, this is a tray icon so don't show it with the window manager
            Execute.OnUIThread(() =>
            {
                UIElement view = viewManager.CreateAndBindViewForModelIfNecessary(RootViewModel);
                ((TrayViewModel) RootViewModel).SetTaskbarIcon(view);
            });

            // Initialize the core async so the UI can show the progress
            Task.Run(async () =>
            {
                try
                {
                    if (StartupArguments.Contains("--autorun"))
                    {
                        logger.Information("Sleeping for 15 seconds on auto run to allow vendor applications required for SDKs to start");
                        await Task.Delay(TimeSpan.FromSeconds(15));
                    }

                    _core.StartupArguments = StartupArguments;
                    _core.Initialize();
                }
                catch (Exception e)
                {
                    HandleFatalException(e, logger);
                    throw;
                }
            });

            Kernel.Get<IRegistrationService>().RegisterInputProvider();
        }

        protected override void ConfigureIoC(IKernel kernel)
        {
            // This is kinda needed for the VM factories in the Shared UI but perhaps there's a less global solution
            kernel.Settings.InjectNonPublic = true;

            // Load the UI modules
            kernel.Load<UIModule>();
            kernel.Load<SharedUIModule>();
            // Load the core assembly's module
            kernel.Load<CoreModule>();

            base.ConfigureIoC(kernel);
        }

        protected override void OnUnhandledException(DispatcherUnhandledExceptionEventArgs e)
        {
            ILogger logger = Kernel.Get<ILogger>();
            logger.Fatal(e.Exception, "Unhandled exception");

            IDialogService dialogService = Kernel.Get<IDialogService>();
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

        private void UtilitiesOnShutdownRequested(object sender, EventArgs e)
        {
            Execute.OnUIThread(() => Application.Current.Shutdown());
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
    }
}
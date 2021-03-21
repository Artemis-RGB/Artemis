using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Threading;
using Artemis.Core;
using Artemis.Core.Ninject;
using Artemis.Core.Services;
using Artemis.UI.Ninject;
using Artemis.UI.Screens;
using Artemis.UI.Services;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Artemis.UI.SkiaSharp;
using Artemis.UI.Stylet;
using Ninject;
using Serilog;
using SkiaSharp;
using Stylet;

namespace Artemis.UI
{
    public class Bootstrapper : NinjectBootstrapper<TrayViewModel>
    {
        private ApplicationStateManager _applicationStateManager;
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
            _applicationStateManager = new ApplicationStateManager(Args);
            Core.Utilities.PrepareFirstLaunch();

            ILogger logger = Kernel.Get<ILogger>();
            if (_applicationStateManager.FocusExistingInstance())
            {
                logger.Information("Shutting down because a different instance is already running.");
                Application.Current.Shutdown(1);
                return;
            }

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

                CreateGraphicsContext();
            });

            // Initialize the core async so the UI can show the progress
            Task.Run(() =>
            {
                try
                {
                    _core.StartupArguments = StartupArguments;
                    _core.IsElevated = _applicationStateManager.IsElevated;
                    _core.Initialize();
                }
                catch (Exception e)
                {
                    HandleFatalException(e, logger);
                    throw;
                }
            });

            IRegistrationService registrationService = Kernel.Get<IRegistrationService>();
            registrationService.RegisterInputProvider();
            registrationService.RegisterControllers();

            // Initialize background services
            Kernel.Get<IDeviceLayoutService>();
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

        private void CreateGraphicsContext()
        {
            Win32VkContext vulkanContext = new();
            GRVkBackendContext vulkanBackendContext = new()
            {
                VkInstance = (IntPtr) vulkanContext.Instance.RawHandle.ToUInt64(),
                VkPhysicalDevice = (IntPtr) vulkanContext.PhysicalDevice.RawHandle.ToUInt64(),
                VkDevice = (IntPtr) vulkanContext.Device.RawHandle.ToUInt64(),
                VkQueue = (IntPtr) vulkanContext.GraphicsQueue.RawHandle.ToUInt64(),
                GraphicsQueueIndex = vulkanContext.GraphicsFamily,
                GetProcedureAddress = vulkanContext.GetProc
            };

            Constants.SkiaGraphicsContext = GRContext.CreateVulkan(vulkanBackendContext);
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
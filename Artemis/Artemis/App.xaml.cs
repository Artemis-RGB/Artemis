using System;
using System.Security.Principal;
using System.Windows;
using System.Windows.Threading;
using Artemis.Utilities;
using NLog;
using WpfExceptionViewer;

namespace Artemis
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            Environment.Exit(0);
        }

        public App()
        {
            if (!IsRunAsAdministrator())
                GeneralHelpers.RunAsAdministrator();

            InitializeComponent();
        }

        public bool DoHandle { get; set; }

        private static bool IsRunAsAdministrator()
        {
            var wi = WindowsIdentity.GetCurrent();
            var wp = new WindowsPrincipal(wi);

            return wp.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (DoHandle)
            {
                GetArtemisExceptionViewer(e.Exception).ShowDialog();
                e.Handled = true;
            }
            else
            {
                GetArtemisExceptionViewer(e.Exception).ShowDialog();
                e.Handled = false;
            }
        }

        private static ExceptionViewer GetArtemisExceptionViewer(Exception e)
        {
            var logger = LogManager.GetCurrentClassLogger();
            logger.Fatal(e, "Unhandled exception, showing dialog and shutting down.");
            return new ExceptionViewer("An unexpected error occurred in Artemis.", e)
            {
                Title = "Artemis - Exception :c",
                Height = 400,
                Width = 800
            };
        }
    }
}
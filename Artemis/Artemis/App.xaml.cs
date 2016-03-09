using System;
using System.Diagnostics;
using System.Reflection;
using System.Security.Principal;
using System.Windows;
using System.Windows.Threading;
using WpfExceptionViewer;

namespace Artemis
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            if (!IsRunAsAdministrator())
            {
                var processInfo = new ProcessStartInfo(Assembly.GetExecutingAssembly().CodeBase);

                // The following properties run the new process as administrator
                processInfo.UseShellExecute = true;
                processInfo.Verb = "runas";

                // Start the new process
                try
                {
                    Process.Start(processInfo);
                }
                catch (Exception)
                {
                    // The user did not allow the application to run as administrator
                    MessageBox.Show("Sorry, this application must be run as Administrator.");
                }

                // Shut down the current process
                Environment.Exit(0);
            }

            InitializeComponent();
        }

        public bool DoHandle { get; set; }

        private bool IsRunAsAdministrator()
        {
            var wi = WindowsIdentity.GetCurrent();
            var wp = new WindowsPrincipal(wi);

            return wp.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
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


        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            GetArtemisExceptionViewer(ex).ShowDialog();
        }

        private static ExceptionViewer GetArtemisExceptionViewer(Exception e)
        {
            return new ExceptionViewer("An unexpected error occurred in Artemis.", e)
            {
                Title = "Artemis - Exception :c",
                Height = 400,
                Width = 800
            };
        }
    }
}
using System;
using System.Windows;
using System.Windows.Threading;
using Artemis.Utilities;
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
            if (!GeneralHelpers.IsRunAsAdministrator())
                GeneralHelpers.RunAsAdministrator();

            InitializeComponent();
        }

        public bool DoHandle { get; set; }

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
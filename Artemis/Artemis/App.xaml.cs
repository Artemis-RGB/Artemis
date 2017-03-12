using System;
using System.Windows;
using System.Windows.Threading;
using Artemis.Utilities.Keyboard;
using NLog;
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
            InitializeComponent();
        }

        public bool DoHandle { get; set; }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            Environment.Exit(0);
        }

        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // Get rid of the keyboard hook in case of a crash, otherwise input freezes up system wide until Artemis is gone
            KeyboardHook.Stop();

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

        public static ExceptionViewer GetArtemisExceptionViewer(Exception e)
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
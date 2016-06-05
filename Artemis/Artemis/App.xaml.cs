﻿using System;
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
        public App()
        {
            //if (!IsRunAsAdministrator())
            //    GeneralHelpers.RunAsAdministrator();

            InitializeComponent();
        }

        private static bool IsRunAsAdministrator()
        {
            var wi = WindowsIdentity.GetCurrent();
            var wp = new WindowsPrincipal(wi);

            return wp.IsInRole(WindowsBuiltInRole.Administrator);
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
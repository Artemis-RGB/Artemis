using System;
using System.Windows;
using Artemis.ViewModels;
using Caliburn.Micro;
using MessageBox = System.Windows.Forms.MessageBox;

namespace Artemis
{
    public class AppBootstrapper : BootstrapperBase
    {
        public AppBootstrapper()
        {
            Initialize();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<ShellViewModel>();
        }
    }
}
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
            try
            {
                DisplayRootViewFor<ShellViewModel>();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Startup failed :c \n" + ex.InnerException.Message);
                throw;
            }
        }
    }
}
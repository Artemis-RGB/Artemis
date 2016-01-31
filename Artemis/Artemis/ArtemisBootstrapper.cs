using System.Windows;
using Artemis.ViewModels;
using Caliburn.Micro;

namespace Artemis
{
    public class ArtemisBootstrapper : BootstrapperBase
    {
        public ArtemisBootstrapper()
        {
            Initialize();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<ShellViewModel>();
        }
    }
}
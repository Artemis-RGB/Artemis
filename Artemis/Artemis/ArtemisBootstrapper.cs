using System.Windows;
using Artemis.ViewModels;
using Autofac;
using Caliburn.Micro;
using Caliburn.Micro.Autofac;

namespace Artemis
{
    public class ArtemisBootstrapper : AutofacBootstrapper<SystemTrayViewModel>
    {
        public ArtemisBootstrapper()
        {
            Initialize();
        }

        protected override void ConfigureContainer(ContainerBuilder builder)
        {
            base.ConfigureContainer(builder);

            // create a window manager instance to be used by everyone asking for one (including Caliburn.Micro)
            builder.RegisterInstance<IWindowManager>(new WindowManager());
            builder.RegisterType<SystemTrayViewModel>();
            builder.RegisterType<ShellViewModel>();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<SystemTrayViewModel>();
        }
    }
}
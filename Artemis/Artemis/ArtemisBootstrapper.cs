using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using Artemis.ViewModels;
using Autofac;
using Caliburn.Micro;
using Caliburn.Micro.Autofac;
using Application = System.Windows.Application;
using MessageBox = System.Windows.Forms.MessageBox;

namespace Artemis
{
    public class ArtemisBootstrapper : AutofacBootstrapper<SystemTrayViewModel>
    {
        public ArtemisBootstrapper()
        {
            CheckDuplicateInstances();
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

        private void CheckDuplicateInstances()
        {
            var processes = Process.GetProcesses();
            if (processes.Count(p => p.ProcessName == "Artemis") < 2)
                return;

            MessageBox.Show("An instance of Artemis is already running (check your system tray).",
                "Artemis  (╯°□°）╯︵ ┻━┻", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            Application.Current.Shutdown();
        }
    }
}
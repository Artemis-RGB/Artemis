using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Artemis.ViewModels;
using Autofac;
using Caliburn.Micro;
using Caliburn.Micro.Autofac;
using Application = System.Windows.Application;
using MessageBox = System.Windows.Forms.MessageBox;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace Artemis
{
    public class ArtemisBootstrapper : AutofacBootstrapper<SystemTrayViewModel>
    {
        public ArtemisBootstrapper()
        {
            CheckDuplicateInstances();

            Initialize();

            MessageBinder.SpecialValues.Add("$scaledmousex", ctx =>
            {
                var img = ctx.Source as Image;
                var input = ctx.Source as IInputElement;
                var e = ctx.EventArgs as MouseEventArgs;

                // If there is an image control, get the scaled position
                if (img != null && e != null)
                {
                    var position = e.GetPosition(img);
                    return (int) (img.Source.Width*(position.X/img.ActualWidth));
                }

                // If there is another type of of IInputControl get the non-scaled position - or do some processing to get a scaled position, whatever needs to happen
                if (e != null && input != null)
                    return e.GetPosition(input).X;

                // Return 0 if no processing could be done
                return 0;
            });
            MessageBinder.SpecialValues.Add("$scaledmousey", ctx =>
            {
                var img = ctx.Source as Image;
                var input = ctx.Source as IInputElement;
                var e = ctx.EventArgs as MouseEventArgs;

                // If there is an image control, get the scaled position
                if (img != null && e != null)
                {
                    var position = e.GetPosition(img);
                    return (int) (img.Source.Width*(position.Y/img.ActualWidth));
                }

                // If there is another type of of IInputControl get the non-scaled position - or do some processing to get a scaled position, whatever needs to happen
                if (e != null && input != null)
                    return e.GetPosition(input).Y;

                // Return 0 if no processing could be done
                return 0;
            });
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
            if (Process.GetProcesses().Count(p => p.ProcessName.Contains(Assembly.GetExecutingAssembly()
                .FullName.Split(',')[0]) && !p.Modules[0].FileName.Contains("vshost")) < 2)
                return;

            MessageBox.Show("An instance of Artemis is already running (check your system tray).",
                "Artemis  (╯°□°）╯︵ ┻━┻", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            Application.Current.Shutdown();
        }
    }
}
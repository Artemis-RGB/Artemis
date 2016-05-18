using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Artemis.InjectionModules;
using Artemis.ViewModels;
using Caliburn.Micro;
using Ninject;
using Application = System.Windows.Application;
using MessageBox = System.Windows.Forms.MessageBox;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace Artemis
{
    public class ArtemisBootstrapper : BootstrapperBase
    {
        private IKernel _kernel;

        public ArtemisBootstrapper()
        {
            CheckDuplicateInstances();
            Initialize();
            BindSpecialValues();
        }

        public Mutex Mutex { get; set; }

        private void BindSpecialValues()
        {
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

        protected override void Configure()
        {
            _kernel = new StandardKernel(new BaseModules(), new ArtemisModules(), new ManagerModules());
            _kernel.Bind<IWindowManager>().To<WindowManager>().InSingletonScope();
            _kernel.Bind<IEventAggregator>().To<EventAggregator>().InSingletonScope();
        }

        protected override void OnExit(object sender, EventArgs e)
        {
            _kernel.Dispose();
            base.OnExit(sender, e);
        }

        protected override object GetInstance(Type service, string key)
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));

            return _kernel.Get(service);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return _kernel.GetAll(service);
        }

        protected override void BuildUp(object instance)
        {
            _kernel.Inject(instance);
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<SystemTrayViewModel>();
        }

        private void CheckDuplicateInstances()
        {
            bool aIsNewInstance;
            Mutex = new Mutex(true, "ArtemisMutex", out aIsNewInstance);
            if (aIsNewInstance)
                return;

            MessageBox.Show("An instance of Artemis is already running (check your system tray).",
                "Artemis  (╯°□°）╯︵ ┻━┻", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            Application.Current.Shutdown();
        }
    }
}
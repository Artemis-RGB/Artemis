using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Artemis.InjectionModules;
using Artemis.Settings;
using Artemis.Utilities;
using Artemis.Utilities.Converters;
using Artemis.ViewModels;
using Caliburn.Micro;
using Newtonsoft.Json;
using Ninject;
using Ninject.Extensions.Logging;

namespace Artemis
{
    public class ArtemisBootstrapper : BootstrapperBase
    {
        private IKernel _kernel;

        public ArtemisBootstrapper()
        {
            // Start logging before anything else
            Logging.SetupLogging(General.Default.LogLevel);

            Initialize();
            BindSpecialValues();
        }

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

            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                ContractResolver = _kernel.Get<NinjectContractResolver>()
            };
            JsonConvert.DefaultSettings = () => settings;
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
    }
}
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Threading;
using Ninject;
using Serilog;
using Stylet;

namespace Artemis.UI.Stylet
{
    public class NinjectBootstrapper<TRootViewModel> : BootstrapperBase where TRootViewModel : class
    {
        private object _rootViewModel;
        protected IKernel Kernel;

        protected virtual object RootViewModel => _rootViewModel ?? (_rootViewModel = GetInstance(typeof(TRootViewModel)));

        protected override void ConfigureBootstrapper()
        {
            Kernel = new StandardKernel();
            DefaultConfigureIoC(Kernel);
            ConfigureIoC(Kernel);
        }


        /// <summary>
        ///     Carries out default configuration of the IoC container. Override if you don't want to do this
        /// </summary>
        protected virtual void DefaultConfigureIoC(IKernel kernel)
        {
            var viewManagerConfig = new ViewManagerConfig
            {
                ViewFactory = GetInstance,
                ViewAssemblies = new List<Assembly> {GetType().Assembly}
            };
            kernel.Bind<IViewManager>().ToConstant(new ViewManager(viewManagerConfig));

            kernel.Bind<IWindowManagerConfig>().ToConstant(this).InTransientScope();
            kernel.Bind<IWindowManager>().ToMethod(
                    c => new WindowManager(c.Kernel.Get<IViewManager>(), () => c.Kernel.Get<IMessageBoxViewModel>(), c.Kernel.Get<IWindowManagerConfig>())
                )
                .InSingletonScope();
            kernel.Bind<IEventAggregator>().To<EventAggregator>().InSingletonScope();
            kernel.Bind<IMessageBoxViewModel>().To<MessageBoxViewModel>(); // Not singleton!
        }

        /// <summary>
        ///     Override to add your own types to the IoC container.
        /// </summary>
        protected virtual void ConfigureIoC(IKernel kernel)
        {
        }

        public override object GetInstance(Type type)
        {
            return Kernel.Get(type);
        }

        protected override void Launch()
        {
        }

        public override void Dispose()
        {
            base.Dispose();
            ScreenExtensions.TryDispose(_rootViewModel);
            if (Kernel != null)
                Kernel.Dispose();
        }

        protected override void OnUnhandledException(DispatcherUnhandledExceptionEventArgs e)
        {
            var logger = Kernel.Get<ILogger>();
            logger.Fatal(e.Exception, "Fatal exception, shutting down.");

            base.OnUnhandledException(e);
        }
    }
}
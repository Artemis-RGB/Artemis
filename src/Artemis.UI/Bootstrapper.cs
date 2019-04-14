using System.Windows;
using Artemis.Core.Ninject;
using Artemis.Core.Services.Interfaces;
using Artemis.UI.Ninject;
using Artemis.UI.Stylet;
using Artemis.UI.ViewModels;
using Ninject;

namespace Artemis.UI
{
    public class Bootstrapper : NinjectBootstrapper<RootViewModel>
    {
        private ICoreService _core;

        protected override void OnExit(ExitEventArgs e)
        {
            // Stop the Artemis core
            _core.Dispose();

            base.OnExit(e);
        }

        protected override void ConfigureIoC(IKernel kernel)
        {
            kernel.Settings.InjectNonPublic = true;

            // Load this assembly's module
            kernel.Load<UIModule>();
            // Load the core assembly's module
            kernel.Load<CoreModule>();

            // Start the Artemis core, the core's constructor will initialize async
            _core = Kernel.Get<ICoreService>();

            base.ConfigureIoC(kernel);
        }
    }
}
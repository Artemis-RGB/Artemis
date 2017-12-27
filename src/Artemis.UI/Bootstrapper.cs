using Artemis.UI.Ninject;
using Artemis.UI.Stylet;
using Artemis.UI.ViewModels;
using Ninject;

namespace Artemis.UI
{
    public class Bootstrapper : NinjectBootstrapper<RootViewModel>
    {
        protected override void ConfigureIoC(IKernel kernel)
        {
            kernel.Load<UIModule>();
            base.ConfigureIoC(kernel);
        }
    }
}
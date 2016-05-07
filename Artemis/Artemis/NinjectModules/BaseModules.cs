using Artemis.ViewModels;
using Artemis.ViewModels.Abstract;
using Caliburn.Micro;
using Ninject.Modules;

namespace Artemis.NinjectModules
{
    internal class BaseModules : NinjectModule
    {
        public override void Load()
        {
            // ViewModels
            Bind<IScreen>().To<ShellViewModel>().InSingletonScope();

            Bind<BaseViewModel>().To<EffectsViewModel>().InSingletonScope();
            Bind<BaseViewModel>().To<GamesViewModel>().InSingletonScope();
            Bind<BaseViewModel>().To<OverlaysViewModel>().InSingletonScope();
        }
    }
}
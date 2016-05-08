using Artemis.InjectionFactories;
using Artemis.Modules.Effects.ProfilePreview;
using Artemis.ViewModels;
using Artemis.ViewModels.Abstract;
using Caliburn.Micro;
using Ninject.Extensions.Factory;
using Ninject.Modules;

namespace Artemis.InjectionModules
{
    internal class BaseModules : NinjectModule
    {
        public override void Load()
        {
            // ViewModels
            Bind<IScreen>().To<ShellViewModel>().InSingletonScope();
            Bind<IProfileEditorViewModelFactory>().ToFactory();

            Bind<BaseViewModel>().To<EffectsViewModel>().InSingletonScope();
            Bind<BaseViewModel>().To<GamesViewModel>().InSingletonScope();
            Bind<BaseViewModel>().To<OverlaysViewModel>().InSingletonScope();

            // Models
            Bind<ProfilePreviewModel>().ToSelf().InSingletonScope();
        }
    }
}
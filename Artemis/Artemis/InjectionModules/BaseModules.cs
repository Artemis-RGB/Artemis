using Artemis.InjectionFactories;
using Artemis.Modules.Effects.ProfilePreview;
using Artemis.Services;
using Artemis.ViewModels;
using Artemis.ViewModels.Abstract;
using Artemis.ViewModels.Profiles;
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
            Bind<ProfileViewModel>().ToSelf();

            Bind<BaseViewModel>().To<WelcomeViewModel>().InSingletonScope();
            Bind<BaseViewModel>().To<EffectsViewModel>().InSingletonScope();
            Bind<BaseViewModel>().To<GamesViewModel>().InSingletonScope();
            Bind<BaseViewModel>().To<OverlaysViewModel>().InSingletonScope();

            // Models
            Bind<ProfilePreviewModel>().ToSelf().InSingletonScope();

            // Services
            Bind<MetroDialogService>().ToSelf().InSingletonScope();
        }
    }
}
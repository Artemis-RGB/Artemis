using Artemis.Modules.Effects.ProfilePreview;
using Artemis.Services;
using Artemis.Utilities.DataReaders;
using Artemis.Utilities.GameState;
using Artemis.ViewModels;
using Artemis.ViewModels.Abstract;
using Artemis.ViewModels.Profiles;
using Ninject.Modules;

namespace Artemis.InjectionModules
{
    internal class BaseModules : NinjectModule
    {
        public override void Load()
        {
            // ViewModels
            Bind<ShellViewModel>().ToSelf();
            Bind<ProfileViewModel>().ToSelf();
            Bind<ProfileEditorViewModel>().ToSelf();
            Bind<DebugViewModel>().ToSelf().InSingletonScope();

            Bind<BaseViewModel>().To<WelcomeViewModel>();
            Bind<BaseViewModel>().To<EffectsViewModel>();
            Bind<BaseViewModel>().To<GamesViewModel>();
            Bind<BaseViewModel>().To<OverlaysViewModel>();

            // Models
            Bind<ProfilePreviewModel>().ToSelf().InSingletonScope();

            // Services
            Bind<MetroDialogService>().ToSelf().InSingletonScope();
            Bind<WindowService>().ToSelf().InSingletonScope();

            // Servers
            Bind<GameStateWebServer>().ToSelf().InSingletonScope();
            Bind<PipeServer>().ToSelf().InSingletonScope();
        }
    }
}
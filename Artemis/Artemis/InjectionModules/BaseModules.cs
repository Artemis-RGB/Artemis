using Artemis.DeviceProviders;
using Artemis.Models;
using Artemis.Modules.Effects.ProfilePreview;
using Artemis.Profiles.Layers.Interfaces;
using Artemis.Profiles.Layers.Types.AmbientLight;
using Artemis.Profiles.Layers.Types.Audio;
using Artemis.Profiles.Layers.Types.Audio.AudioCapturing;
using Artemis.Profiles.Layers.Types.KeyPress;
using Artemis.Profiles.Lua;
using Artemis.Services;
using Artemis.Utilities.DataReaders;
using Artemis.Utilities.GameState;
using Artemis.ViewModels;
using Artemis.ViewModels.Abstract;
using Artemis.ViewModels.Profiles;
using Ninject.Extensions.Conventions;
using Ninject.Modules;

namespace Artemis.InjectionModules
{
    public class BaseModules : NinjectModule
    {
        public override void Load()
        {
            #region ViewModels

            Bind<ShellViewModel>().ToSelf().InSingletonScope();
            Bind<ProfileViewModel>().ToSelf();
            Bind<ProfileEditorViewModel>().ToSelf();
            Bind<DebugViewModel>().ToSelf().InSingletonScope();
            Kernel.Bind(x =>
                x.FromThisAssembly()
                    .SelectAllClasses()
                    .InheritedFrom<BaseViewModel>()
                    .BindAllBaseClasses());

            #endregion

            #region Models

            Bind<ProfilePreviewModel>().ToSelf().InSingletonScope();

            #endregion

            #region Services

            Bind<MetroDialogService>().ToSelf().InSingletonScope();
            Bind<WindowService>().ToSelf().InSingletonScope();

            #endregion

            #region Servers

            Bind<GameStateWebServer>().ToSelf().InSingletonScope();
            Bind<PipeServer>().ToSelf().InSingletonScope();

            #endregion

            #region Devices

            Kernel.Bind(x =>
                x.FromThisAssembly()
                    .SelectAllClasses()
                    .InheritedFrom<DeviceProvider>()
                    .BindAllBaseClasses());

            #endregion

            #region Effects

            Kernel.Bind(x =>
                x.FromThisAssembly()
                    .SelectAllClasses()
                    .InheritedFrom<EffectModel>()
                    .BindAllBaseClasses()
                    .Configure((b, c) => b.InSingletonScope().Named(c.Name))
            );
            Kernel.Bind(x =>
                x.FromThisAssembly()
                    .SelectAllClasses()
                    .InheritedFrom<EffectViewModel>()
                    .BindBase());
            Kernel.Bind(x =>
                x.FromThisAssembly()
                    .SelectAllClasses()
                    .InheritedFrom<GameViewModel>()
                    .BindBase());
            Kernel.Bind(x =>
                x.FromThisAssembly()
                    .SelectAllClasses()
                    .InheritedFrom<OverlayViewModel>()
                    .BindBase());

            #endregion

            #region Profiles

            Kernel.Bind(x =>
                x.FromThisAssembly()
                    .SelectAllClasses()
                    .InheritedFrom<ILayerAnimation>()
                    .BindAllInterfaces());
            Kernel.Bind(x =>
                x.FromThisAssembly()
                    .SelectAllClasses()
                    .InheritedFrom<ILayerCondition>()
                    .BindAllInterfaces());
            Kernel.Bind(x =>
                x.FromThisAssembly()
                    .SelectAllClasses()
                    .InheritedFrom<ILayerType>()
                    .BindAllInterfaces());
            Kernel.Bind(x =>
                x.FromThisAssembly()
                    .SelectAllClasses()
                    .InheritedFrom<ILayerType>()
                    .BindToSelf());
            
            // Type helpers
            Bind<AudioCaptureManager>().ToSelf().InSingletonScope();

            #endregion

            #region Lua

            Kernel.Bind(x =>
                x.FromThisAssembly()
                    .SelectAllClasses()
                    .InheritedFrom<LuaModule>()
                    .BindAllBaseClasses());

            #endregion
        }
    }
}
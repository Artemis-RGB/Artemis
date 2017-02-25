using Artemis.DeviceProviders;
using Artemis.Managers;
using Artemis.Models;
using Artemis.Modules.Abstract;
using Artemis.Profiles.Layers.Abstract;
using Artemis.Profiles.Layers.Interfaces;
using Artemis.Profiles.Layers.Types.Audio.AudioCapturing;
using Artemis.Profiles.Lua;
using Artemis.Services;
using Artemis.Utilities.DataReaders;
using Artemis.Utilities.GameState;
using Artemis.ViewModels;
using Artemis.ViewModels.Abstract;
using Ninject.Extensions.Conventions;
using Ninject.Modules;

namespace Artemis.InjectionModules
{
    public class BaseModules : NinjectModule
    {
        public override void Load()
        {
            #region Models

            Bind<ProfileEditorModel>().ToSelf();
            Bind<LayerEditorModel>().ToSelf();

            #endregion

            #region ViewModels

            Bind<ShellViewModel>().ToSelf().InSingletonScope();
            Bind<ProfileEditorViewModel>().ToSelf();
            Bind<DebugViewModel>().ToSelf().InSingletonScope();
            Kernel.Bind(x =>
                x.FromThisAssembly()
                    .SelectAllClasses()
                    .InheritedFrom<BaseViewModel>()
                    .BindAllBaseClasses());

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
                    .InheritedFrom<ModuleModel>()
                    .BindAllBaseClasses()
                    .Configure((b, c) => b.InSingletonScope().Named(c.Name))
            );
            Kernel.Bind(x =>
                x.FromThisAssembly()
                    .SelectAllClasses()
                    .InheritedFrom<ModuleViewModel>()
                    .BindAllBaseClasses()
                    .Configure(b => b.InSingletonScope())
            );

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
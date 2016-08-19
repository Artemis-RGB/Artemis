using Artemis.Models;
using Artemis.ViewModels.Abstract;
using Ninject.Extensions.Conventions;
using Ninject.Modules;

namespace Artemis.InjectionModules
{
    public class EffectModules : NinjectModule
    {
        public override void Load()
        {
            // Effects
            Kernel.Bind(x =>
            {
                x.FromThisAssembly()
                    .SelectAllClasses()
                    .InheritedFrom<EffectModel>()
                    .BindBase()
                    .Configure(b => b.InSingletonScope());
            });

            Kernel.Bind(x =>
            {
                x.FromThisAssembly()
                    .SelectAllClasses()
                    .InheritedFrom<EffectViewModel>()
                    .BindBase()
                    .Configure(b => b.InSingletonScope());
            });

            // Games
            Kernel.Bind(x =>
            {
                x.FromThisAssembly()
                    .SelectAllClasses()
                    .InheritedFrom<GameModel>()
                    .BindBase()
                    .Configure(b => b.InSingletonScope());
            });

            Kernel.Bind(x =>
            {
                x.FromThisAssembly()
                    .SelectAllClasses()
                    .InheritedFrom<GameViewModel>()
                    .BindBase()
                    .Configure(b => b.InSingletonScope());
            });

            // Overlays
            Kernel.Bind(x =>
            {
                x.FromThisAssembly()
                    .SelectAllClasses()
                    .InheritedFrom<OverlayModel>()
                    .BindBase()
                    .Configure(b => b.InSingletonScope());
            });

            Kernel.Bind(x =>
            {
                x.FromThisAssembly()
                    .SelectAllClasses()
                    .InheritedFrom<OverlayViewModel>()
                    .BindBase()
                    .Configure(b => b.InSingletonScope());
            });
        }
    }
}
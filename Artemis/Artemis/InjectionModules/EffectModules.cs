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
                    .Configure((b, c) => b.InSingletonScope().Named(c.Name));
            });

            // View models
            Kernel.Bind(x =>
            {
                x.FromThisAssembly()
                    .SelectAllClasses()
                    .InheritedFrom<EffectViewModel>()
                    .BindBase();
            });

            Kernel.Bind(x =>
            {
                x.FromThisAssembly()
                    .SelectAllClasses()
                    .InheritedFrom<GameViewModel>()
                    .BindBase();
            });
            Kernel.Bind(x =>
            {
                x.FromThisAssembly()
                    .SelectAllClasses()
                    .InheritedFrom<OverlayViewModel>()
                    .BindBase();
            });
        }
    }
}
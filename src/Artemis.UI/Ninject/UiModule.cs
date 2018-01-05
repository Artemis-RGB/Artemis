using Artemis.UI.ViewModels.Interfaces;
using Ninject.Extensions.Conventions;
using Ninject.Modules;

namespace Artemis.UI.Ninject
{
    // ReSharper disable once InconsistentNaming
    public class UIModule : NinjectModule
    {
        public override void Load()
        {
            // Bind all built-in viewmodels
            Kernel.Bind(x =>
            {
                x.FromThisAssembly()
                    .SelectAllClasses()
                    .InheritedFrom<IArtemisViewModel>()
                    .BindAllInterfaces();
            });
        }
    }
}
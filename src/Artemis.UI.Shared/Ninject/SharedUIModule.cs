using System;
using Artemis.UI.Shared.Ninject.Factories;
using Artemis.UI.Shared.Services.Interfaces;
using MaterialDesignThemes.Wpf;
using Ninject.Extensions.Conventions;
using Ninject.Modules;

namespace Artemis.UI.Shared.Ninject
{
    // ReSharper disable once InconsistentNaming
    public class SharedUIModule : NinjectModule
    {
        public override void Load()
        {
            if (Kernel == null)
                throw new ArgumentNullException("Kernel shouldn't be null here.");

            // Bind UI factories
            Kernel.Bind(x =>
            {
                x.FromAssemblyContaining<IVmFactory>()
                    .SelectAllInterfaces()
                    .InheritedFrom<IVmFactory>()
                    .BindToFactory();
            });

            // Bind all shared UI services as singletons
            Kernel.Bind(x =>
            {
                x.FromAssemblyContaining<IArtemisSharedUIService>()
                    .SelectAllClasses()
                    .InheritedFrom<IArtemisSharedUIService>()
                    .BindAllInterfaces()
                    .Configure(c => c.InSingletonScope());
            });

            Kernel.Bind<ISnackbarMessageQueue>().ToConstant(new SnackbarMessageQueue()).InSingletonScope();
        }
    }
}
using System;
using Artemis.UI.Avalonia.Ninject.Factories;
using Artemis.UI.Avalonia.Screens;
using Ninject.Extensions.Conventions;
using Ninject.Modules;

namespace Artemis.UI.Avalonia.Ninject
{
    public class UIModule : NinjectModule
    {
        public override void Load()
        {
            if (Kernel == null)
                throw new ArgumentNullException("Kernel shouldn't be null here.");


            Kernel.Bind(x =>
            {
                x.FromThisAssembly()
                    .SelectAllClasses()
                    .InheritedFrom<ViewModelBase>()
                    .BindToSelf();
            });

            Kernel.Bind(x =>
            {
                x.FromThisAssembly()
                    .SelectAllClasses()
                    .InheritedFrom<MainScreenViewModel>()
                    .BindAllBaseClasses();
            });

            // Bind UI factories
            Kernel.Bind(x =>
            {
                x.FromThisAssembly()
                    .SelectAllInterfaces()
                    .InheritedFrom<IVmFactory>()
                    .BindToFactory();
            });
        }
    }
}
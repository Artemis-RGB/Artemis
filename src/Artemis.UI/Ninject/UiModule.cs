using System;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Ninject.InstanceProviders;
using Artemis.UI.Screens;
using Artemis.UI.Screens.ProfileEditor;
using Artemis.UI.Services.Interfaces;
using Artemis.UI.Shared.Services;
using Artemis.UI.Stylet;
using FluentValidation;
using Ninject.Extensions.Conventions;
using Ninject.Extensions.Factory;
using Ninject.Modules;
using Stylet;

namespace Artemis.UI.Ninject
{
    public class UIModule : NinjectModule
    {
        public override void Load()
        {
            if (Kernel == null)
                throw new ArgumentNullException("Kernel shouldn't be null here.");

            // Bind all built-in VMs
            Kernel.Bind(x =>
            {
                x.FromThisAssembly()
                    .SelectAllClasses()
                    .InheritedFrom<IMainScreenViewModel>()
                    .BindAllBaseClasses()
                    .Configure(c => c.InSingletonScope());
            });

            // Bind all dialog VMs
            Kernel.Bind(x =>
            {
                x.FromThisAssembly()
                    .SelectAllClasses()
                    .InheritedFrom<DialogViewModelBase>()
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

            Kernel.Bind<IDataBindingsVmFactory>().ToFactory(() => new DataBindingsViewModelInstanceProvider());
            Kernel.Bind<IPropertyVmFactory>().ToFactory(() => new LayerPropertyViewModelInstanceProvider());

            // Bind profile editor VMs
            Kernel.Bind(x =>
            {
                x.FromThisAssembly()
                    .SelectAllClasses()
                    .InheritedFrom<IProfileEditorPanelViewModel>()
                    .BindAllBaseClasses();
            });

            // Bind all UI services as singletons
            Kernel.Bind(x =>
            {
                x.FromThisAssembly()
                    .SelectAllClasses()
                    .InheritedFrom<IArtemisUIService>()
                    .BindAllInterfaces()
                    .Configure(c => c.InSingletonScope());
            });

            // Bind all validators
            Bind(typeof(IModelValidator<>)).To(typeof(FluentValidationAdapter<>));
            Kernel.Bind(x =>
            {
                x.FromThisAssembly()
                    .SelectAllClasses()
                    .InheritedFrom<IValidator>()
                    .BindAllInterfaces();
            });
        }
    }
}
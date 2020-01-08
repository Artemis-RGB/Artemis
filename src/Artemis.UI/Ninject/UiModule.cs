using System;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Screens;
using Artemis.UI.Screens.Module.ProfileEditor;
using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.PropertyTree.PropertyInput;
using Artemis.UI.Services.Interfaces;
using Artemis.UI.Stylet;
using Artemis.UI.ViewModels.Dialogs;
using FluentValidation;
using Ninject.Extensions.Conventions;
using Ninject.Modules;
using Stylet;

namespace Artemis.UI.Ninject
{
    // ReSharper disable once InconsistentNaming
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
                    .InheritedFrom<MainScreenViewModel>()
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
                    .InheritedFrom<IViewModelFactory>()
                    .BindToFactory();
            });

            // Bind profile editor VMs
            Kernel.Bind(x =>
            {
                x.FromThisAssembly()
                    .SelectAllClasses()
                    .InheritedFrom<ProfileEditorPanelViewModel>()
                    .BindAllBaseClasses();
            });

            // Bind property input VMs
            Kernel.Bind(x =>
            {
                x.FromThisAssembly()
                    .SelectAllClasses()
                    .InheritedFrom<PropertyInputViewModel>()
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
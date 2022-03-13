using System;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Ninject.InstanceProviders;
using Artemis.UI.Screens;
using Artemis.UI.Screens.ProfileEditor.VisualEditor.Tools;
using Artemis.UI.Services.Interfaces;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services.ProfileEditor;
using Avalonia.Platform;
using Avalonia.Shared.PlatformSupport;
using Ninject.Extensions.Conventions;
using Ninject.Extensions.Factory;
using Ninject.Modules;
using Ninject.Planning.Bindings.Resolvers;

namespace Artemis.UI.Ninject
{
    public class UIModule : NinjectModule
    {
        public override void Load()
        {
            if (Kernel == null)
                throw new ArgumentNullException("Kernel shouldn't be null here.");

            Kernel.Components.Add<IMissingBindingResolver, SelfBindingResolver>();
            Kernel.Bind<IAssetLoader>().ToConstant(new AssetLoader());

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

            Kernel.Bind(x =>
            {
                x.FromThisAssembly()
                    .SelectAllClasses()
                    .InheritedFrom<IToolViewModel>()
                    .BindAllInterfaces();
            });

            // Bind UI factories
            Kernel.Bind(x =>
            {
                x.FromThisAssembly()
                    .SelectAllInterfaces()
                    .InheritedFrom<IVmFactory>()
                    .BindToFactory();
            });

            Kernel.Bind<IPropertyVmFactory>().ToFactory(() => new LayerPropertyViewModelInstanceProvider());
            Kernel.Bind<INodePinVmFactory>().ToFactory(() => new NodePinViewModelInstanceProvider());

            // Bind all UI services as singletons
            Kernel.Bind(x =>
            {
                x.FromThisAssembly()
                    .SelectAllClasses()
                    .InheritedFrom<IArtemisUIService>()
                    .BindAllInterfaces()
                    .Configure(c => c.InSingletonScope());
            });
        }
    }
}
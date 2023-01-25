using Artemis.Core.DryIoc;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Ninject.InstanceProviders;
using Artemis.UI.Screens;
using Artemis.UI.Screens.VisualScripting;
using Artemis.UI.Services.Interfaces;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services.NodeEditor;
using Artemis.UI.Shared.Services.ProfileEditor;
using Avalonia.Platform;
using Avalonia.Shared.PlatformSupport;
using DryIoc;

namespace Artemis.UI.Ninject;

public class UIModule : IModule
{
    public void Load(IRegistrator builder)
    {
        var thisAssembly = typeof(UIModule).Assembly;

        builder.RegisterInstance(new AssetLoader(), IfAlreadyRegistered.Throw);
        builder.Register<IAssetLoader, AssetLoader>(Reuse.Singleton);
        builder.RegisterMany(new[] { thisAssembly }, type => type.IsAssignableTo<ViewModelBase>());
        builder.RegisterMany(new[] { thisAssembly }, type => type.IsAssignableTo<MainScreenViewModel>());
        //builder.RegisterMany(new[] { thisAssembly }, type => type.IsInterface && type.GetInterfaces().Contains(typeof(IVmFactory)));
        builder.RegisterMany<IVmFactory>(nonPublicServiceTypes: true, serviceTypeCondition: t => t.GetGenericDefinitionOrNull);
        builder.RegisterMany(new[] { thisAssembly }, type => type.IsAssignableTo<IToolViewModel>());
        

        builder.Register<NodeScriptWindowViewModelBase, NodeScriptWindowViewModel>(Reuse.Singleton);
        builder.Register<IPropertyVmFactory, PropertyVmFactory>(Reuse.Singleton);

        builder.RegisterMany(new[] { thisAssembly }, type => type.IsAssignableTo<IArtemisUIService>(), Reuse.Singleton);
    }
}
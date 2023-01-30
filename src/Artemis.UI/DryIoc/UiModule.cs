using System.Reflection;
using Artemis.Core.DryIoc;
using Artemis.UI.DryIoc.Factories;
using Artemis.UI.DryIoc.InstanceProviders;
using Artemis.UI.Screens;
using Artemis.UI.Screens.VisualScripting;
using Artemis.UI.Services.Interfaces;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services.NodeEditor;
using Artemis.UI.Shared.Services.ProfileEditor;
using Avalonia.Platform;
using Avalonia.Shared.PlatformSupport;
using DryIoc;

namespace Artemis.UI.DryIoc;

public class UIModule : IModule
{
    public void Load(IRegistrator builder)
    {
        Assembly thisAssembly = typeof(UIModule).Assembly;

        builder.RegisterInstance(new AssetLoader(), IfAlreadyRegistered.Throw);
        builder.Register<IAssetLoader, AssetLoader>(Reuse.Singleton);
        
        builder.RegisterMany(new[] { thisAssembly }, type => type.IsAssignableTo<ViewModelBase>());
        builder.RegisterMany(new[] { thisAssembly }, type => type.IsAssignableTo<MainScreenViewModel>(), ifAlreadyRegistered: IfAlreadyRegistered.Replace);
        builder.RegisterMany(new[] { thisAssembly }, type => type.IsAssignableTo<IToolViewModel>() && type.IsInterface);
        builder.RegisterMany(new[] { thisAssembly }, type => type.IsAssignableTo<IVmFactory>() && type != typeof(PropertyVmFactory));

        builder.Register<NodeScriptWindowViewModelBase, NodeScriptWindowViewModel>(Reuse.Singleton);
        builder.Register<IPropertyVmFactory, PropertyVmFactory>(Reuse.Singleton);

        builder.RegisterMany(new[] { thisAssembly }, type => type.IsAssignableTo<IArtemisUIService>(), Reuse.Singleton);
    }
}
using System.Reflection;
using Artemis.UI.DryIoc.Factories;
using Artemis.UI.DryIoc.InstanceProviders;
using Artemis.UI.Screens;
using Artemis.UI.Screens.VisualScripting;
using Artemis.UI.Services.Interfaces;
using Artemis.UI.Services.Updating;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services.NodeEditor;
using Artemis.UI.Shared.Services.ProfileEditor;
using Avalonia.Platform;
using Avalonia.Shared.PlatformSupport;
using DryIoc;

namespace Artemis.UI.DryIoc;

/// <summary>
/// Provides an extension method to register services onto a DryIoc <see cref="IContainer"/>.
/// </summary>
public static class ContainerExtensions
{
    /// <summary>
    /// Registers UI services into the container.
    /// </summary>
    /// <param name="container">The builder building the current container</param>
    public static void RegisterUI(this IContainer container)
    {
        Assembly[] thisAssembly = {typeof(ContainerExtensions).Assembly};

        container.RegisterInstance(new AssetLoader(), IfAlreadyRegistered.Throw);
        container.Register<IAssetLoader, AssetLoader>(Reuse.Singleton);
        
        container.RegisterMany(thisAssembly, type => type.IsAssignableTo<ViewModelBase>());
        container.RegisterMany(thisAssembly, type => type.IsAssignableTo<IToolViewModel>() && type.IsInterface);
        container.RegisterMany(thisAssembly, type => type.IsAssignableTo<IVmFactory>() && type != typeof(PropertyVmFactory));

        container.Register<NodeScriptWindowViewModelBase, NodeScriptWindowViewModel>(Reuse.Singleton);
        container.Register<IPropertyVmFactory, PropertyVmFactory>(Reuse.Singleton);
        container.Register<IUpdateNotificationProvider, InAppUpdateNotificationProvider>();
        
        container.RegisterMany(thisAssembly, type => type.IsAssignableTo<IArtemisUIService>(), Reuse.Singleton);
    }
}
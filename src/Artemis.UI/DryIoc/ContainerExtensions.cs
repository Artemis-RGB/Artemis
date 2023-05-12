using System.Reflection;
using Artemis.UI.DryIoc.Factories;
using Artemis.UI.DryIoc.InstanceProviders;
using Artemis.UI.Screens.VisualScripting;
using Artemis.UI.Services.Interfaces;
using Artemis.UI.Services.Updating;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services.NodeEditor;
using Artemis.UI.Shared.Services.ProfileEditor;
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
        
        container.RegisterMany(thisAssembly, type => type.IsAssignableTo<ViewModelBase>(), setup: Setup.With(preventDisposal: true));
        container.RegisterMany(thisAssembly, type => type.IsAssignableTo<IToolViewModel>() && type.IsInterface, setup: Setup.With(preventDisposal: true));
        container.RegisterMany(thisAssembly, type => type.IsAssignableTo<IVmFactory>() && type != typeof(PropertyVmFactory));

        container.Register<NodeScriptWindowViewModelBase, NodeScriptWindowViewModel>(Reuse.Singleton);
        container.Register<IPropertyVmFactory, PropertyVmFactory>(Reuse.Singleton);
        container.Register<IUpdateNotificationProvider, BasicUpdateNotificationProvider>();
        
        container.RegisterMany(thisAssembly, type => type.IsAssignableTo<IArtemisUIService>(), Reuse.Singleton);
    }
}
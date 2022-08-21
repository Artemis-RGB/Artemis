using System;
using Artemis.Core.Services;
using Ninject.Extensions.Conventions;
using Ninject.Modules;
using Ninject.Planning.Bindings.Resolvers;

namespace Artemis.Core.Ninject;

internal class PluginModule : NinjectModule
{
    public PluginModule(Plugin plugin)
    {
        Plugin = plugin ?? throw new ArgumentNullException(nameof(plugin));
    }

    public Plugin Plugin { get; }

    public override void Load()
    {
        if (Kernel == null)
            throw new ArtemisCoreException("Failed to bind plugin child module, kernel is null.");

        Kernel.Components.Remove<IMissingBindingResolver, SelfBindingResolver>();

        Kernel.Bind<Plugin>().ToConstant(Plugin).InTransientScope();

        // Bind plugin service interfaces
        Kernel.Bind(x =>
        {
            x.From(Plugin.Assembly)
                .IncludingNonPublicTypes()
                .SelectAllClasses()
                .InheritedFrom<IPluginService>()
                .BindAllInterfaces()
                .Configure(c => c.InSingletonScope());
        });

        // Plugin developers may not use an interface so bind the plugin services to themselves
        // Sadly if they do both, the kernel will treat the interface and the base type as two different singletons
        // perhaps we can avoid that, but I'm not sure how 
        Kernel.Bind(x =>
        {
            x.From(Plugin.Assembly)
                .IncludingNonPublicTypes()
                .SelectAllClasses()
                .InheritedFrom<IPluginService>()
                .BindToSelf()
                .Configure(c => c.InSingletonScope());
        });
    }
}
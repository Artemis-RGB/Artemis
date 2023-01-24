using System;
using Artemis.Core.Services;
using DryIoc;

namespace Artemis.Core.DryIoc;

internal class PluginModule : IModule
{
    public PluginModule(Plugin plugin)
    {
        Plugin = plugin ?? throw new ArgumentNullException(nameof(plugin));
    }

    public Plugin Plugin { get; }

    /// <inheritdoc />
    public void Load(IRegistrator builder)
    {
        builder.RegisterInstance(Plugin, setup: Setup.With(preventDisposal: true));
        
        // Bind plugin service interfaces
        builder.RegisterMany(new[] {Plugin.Assembly}, type => type.IsAssignableTo<IPluginService>(), Reuse.Singleton);
        
        // TODO: Investigate how DryIoc handles the following scenario
        // Plugin developers may not use an interface so bind the plugin services to themselves
        // Sadly if they do both, the kernel will treat the interface and the base type as two different singletons
        // perhaps we can avoid that, but I'm not sure how 
    }
}
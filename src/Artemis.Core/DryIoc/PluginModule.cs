using System;
using System.Linq;
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
        
        // Bind plugin service interfaces, DryIoc expects at least one match when calling RegisterMany so ensure there is something to register first
        if (Plugin.Assembly != null && Plugin.Assembly.GetTypes().Any(t => t.IsAssignableTo<IPluginService>()))
            builder.RegisterMany(new[] {Plugin.Assembly}, type => type.IsAssignableTo<IPluginService>(), Reuse.Singleton, ifAlreadyRegistered: IfAlreadyRegistered.Keep);
    }
}
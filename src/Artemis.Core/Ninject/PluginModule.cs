using System;
using Ninject.Modules;

namespace Artemis.Core.Ninject
{
    internal class PluginModule : NinjectModule
    {
        public PluginModule(PluginInfo pluginInfo)
        {
            PluginInfo = pluginInfo ?? throw new ArgumentNullException(nameof(pluginInfo));
        }

        public PluginInfo PluginInfo { get; }

        public override void Load()
        {
        }
    }
}
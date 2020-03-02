using System.Linq;
using Artemis.Core.Exceptions;
using Artemis.Core.Plugins.Abstract;
using Artemis.Core.Plugins.Models;
using Artemis.Storage.Repositories.Interfaces;
using Ninject.Activation;

namespace Artemis.Core.Ninject
{
    internal class PluginSettingsProvider : Provider<PluginSettings>
    {
        private readonly IPluginRepository _pluginRepository;

        internal PluginSettingsProvider(IPluginRepository pluginRepository)
        {
            _pluginRepository = pluginRepository;
        }

        protected override PluginSettings CreateInstance(IContext context)
        {
            var parentRequest = context.Request.ParentRequest;
            if (parentRequest == null || !typeof(Plugin).IsAssignableFrom(parentRequest.Service))
                throw new ArtemisCoreException("PluginSettings can only be injected into a plugin");
            var pluginInfo = parentRequest.Parameters.FirstOrDefault(p => p.Name == "PluginInfo")?.GetValue(context, null) as PluginInfo;
            if (pluginInfo == null)
                throw new ArtemisCoreException("A plugin needs to be initialized with PluginInfo as a parameter");

            return new PluginSettings(pluginInfo, _pluginRepository);
        }
    }
}
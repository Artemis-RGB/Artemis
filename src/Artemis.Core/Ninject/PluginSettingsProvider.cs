using System.Linq;
using Artemis.Core.Services;
using Artemis.Storage.Repositories.Interfaces;
using Ninject.Activation;

namespace Artemis.Core.Ninject
{
    internal class PluginSettingsProvider : Provider<PluginSettings>
    {
        private readonly IPluginRepository _pluginRepository;
        private readonly IPluginService _pluginService;

        internal PluginSettingsProvider(IPluginRepository pluginRepository, IPluginService pluginService)
        {
            _pluginRepository = pluginRepository;
            _pluginService = pluginService;
        }

        protected override PluginSettings CreateInstance(IContext context)
        {
            IRequest parentRequest = context.Request.ParentRequest;
            if (parentRequest == null)
                throw new ArtemisCoreException("PluginSettings couldn't be injected, failed to get the injection parent request");

            // First try by PluginInfo parameter
            PluginInfo pluginInfo = parentRequest.Parameters.FirstOrDefault(p => p.Name == "PluginInfo")?.GetValue(context, null) as PluginInfo;
            if (pluginInfo == null)
                pluginInfo = _pluginService.GetPluginByAssembly(parentRequest.Service.Assembly)?.PluginInfo;
            // Fall back to assembly based detection
            if (pluginInfo == null)
            {
                throw new ArtemisCoreException("PluginSettings can only be injected with the PluginInfo parameter provided " +
                                               "or into a class defined in a plugin assembly");
            }

            return new PluginSettings(pluginInfo, _pluginRepository);
        }
    }
}
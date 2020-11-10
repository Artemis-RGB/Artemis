using System.Collections.Generic;
using System.Linq;
using Artemis.Core.Services;
using Artemis.Storage.Repositories.Interfaces;
using Ninject.Activation;

namespace Artemis.Core.Ninject
{
    internal class PluginSettingsProvider : Provider<PluginSettings>
    {
        private static readonly List<PluginSettings> PluginSettings = new List<PluginSettings>();
        private readonly IPluginRepository _pluginRepository;
        private readonly IPluginManagementService _pluginManagementService;

        internal PluginSettingsProvider(IPluginRepository pluginRepository, IPluginManagementService pluginManagementService)
        {
            _pluginRepository = pluginRepository;
            _pluginManagementService = pluginManagementService;
        }

        protected override PluginSettings CreateInstance(IContext context)
        {
            IRequest parentRequest = context.Request.ParentRequest;
            if (parentRequest == null)
                throw new ArtemisCoreException("PluginSettings couldn't be injected, failed to get the injection parent request");

            // First try by PluginInfo parameter
            PluginInfo pluginInfo = parentRequest.Parameters.FirstOrDefault(p => p.Name == "PluginInfo")?.GetValue(context, null) as PluginInfo;
            if (pluginInfo == null)
                pluginInfo = _pluginManagementService.GetPluginByAssembly(parentRequest.Service.Assembly)?.PluginInfo;
            // Fall back to assembly based detection
            if (pluginInfo == null)
                throw new ArtemisCoreException("PluginSettings can only be injected with the PluginInfo parameter provided " +
                                               "or into a class defined in a plugin assembly");

            lock (PluginSettings)
            {
                PluginSettings? existingSettings = PluginSettings.FirstOrDefault(p => p.PluginInfo == pluginInfo);
                if (existingSettings != null)
                    return existingSettings;

                PluginSettings? settings = new PluginSettings(pluginInfo, _pluginRepository);
                PluginSettings.Add(settings);
                return settings;
            }
        }
    }
}
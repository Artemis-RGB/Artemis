using System.Collections.Generic;
using System.Linq;
using Artemis.Core.Services;
using Artemis.Storage.Repositories.Interfaces;
using Ninject.Activation;

namespace Artemis.Core.Ninject
{
    // TODO: Investigate if this can't just be set as a constant on the plugin child kernel
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
            Plugin? plugin = parentRequest.Parameters.FirstOrDefault(p => p.Name == "Plugin")?.GetValue(context, null) as Plugin;
            // Fall back to assembly based detection
            if (plugin == null)
                plugin = _pluginManagementService.GetPluginByAssembly(parentRequest.Service.Assembly);
            
            if (plugin == null)
                throw new ArtemisCoreException("PluginSettings can only be injected with the PluginInfo parameter provided " +
                                               "or into a class defined in a plugin assembly");

            lock (PluginSettings)
            {
                PluginSettings? existingSettings = PluginSettings.FirstOrDefault(p => p.Plugin == plugin);
                if (existingSettings != null)
                    return existingSettings;

                PluginSettings? settings = new PluginSettings(plugin, _pluginRepository);
                PluginSettings.Add(settings);
                return settings;
            }
        }
    }
}
using System;
using EmbedIO;

namespace Artemis.Core.Services
{
    public abstract class PluginEndPoint
    {
        private readonly PluginsModule _pluginsModule;

        internal PluginEndPoint(PluginFeature pluginFeature, string name, PluginsModule pluginsModule)
        {
            _pluginsModule = pluginsModule;
            PluginFeature = pluginFeature;
            Name = name;

            PluginFeature.Disabled += OnDisabled;
        }

        /// <summary>
        ///     Gets the plugin the data model is associated with
        /// </summary>
        public PluginFeature PluginFeature { get; }

        /// <summary>
        ///     Gets the name of the end point
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Gets the full URL of the end point
        /// </summary>
        public string Url => $"{_pluginsModule.BaseRoute}{PluginFeature.Plugin.Guid}/{Name}";

        internal abstract void ProcessRequest(IHttpContext context);

        private void OnDisabled(object? sender, EventArgs e)
        {
            PluginFeature.Disabled -= OnDisabled;
            _pluginsModule.RemovePluginEndPoint(this);
        }
    }
}
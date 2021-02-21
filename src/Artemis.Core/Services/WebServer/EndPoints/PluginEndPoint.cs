using System;
using System.Threading.Tasks;
using EmbedIO;
using Newtonsoft.Json;

namespace Artemis.Core.Services
{
    /// <summary>
    ///     Represents a base type for plugin end points to be targeted by the <see cref="PluginsModule" />
    /// </summary>
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
        ///     Gets the name of the end point
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Gets the full URL of the end point
        /// </summary>
        public string Url => $"{_pluginsModule.ServerUrl?.TrimEnd('/')}{_pluginsModule.BaseRoute}{PluginFeature.Plugin.Guid}/{Name}";

        /// <summary>
        ///     Gets the plugin the end point is associated with
        /// </summary>
        [JsonIgnore]
        public PluginFeature PluginFeature { get; }

        /// <summary>
        ///     Gets the plugin info of the plugin the end point is associated with
        /// </summary>
        public PluginInfo PluginInfo => PluginFeature.Plugin.Info;

        /// <summary>
        ///     Gets the mime type of the input this end point accepts
        /// </summary>
        public string? Accepts { get; protected set; }

        /// <summary>
        ///     Gets the mime type of the output this end point returns
        /// </summary>
        public string? Returns { get; protected set; }

        /// <summary>
        ///     Called whenever the end point has to process a request
        /// </summary>
        /// <param name="context">The HTTP context of the request</param>
        protected abstract Task ProcessRequest(IHttpContext context);

        internal async Task InternalProcessRequest(IHttpContext context)
        {
            await ProcessRequest(context);
        }

        private void OnDisabled(object? sender, EventArgs e)
        {
            PluginFeature.Disabled -= OnDisabled;
            _pluginsModule.RemovePluginEndPoint(this);
        }
    }
}
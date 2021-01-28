using System;
using EmbedIO;

namespace Artemis.Core.Services
{
    /// <summary>
    ///     Represents a plugin web endpoint that handles a raw <see cref="IHttpContext" />.
    ///     <para>
    ///         Note: This requires that you reference the EmbedIO
    ///         <see href="https://www.nuget.org/packages/embedio">Nuget package</see>.
    ///     </para>
    /// </summary>
    public class RawPluginEndPoint : PluginEndPoint
    {
        /// <inheritdoc />
        internal RawPluginEndPoint(PluginFeature pluginFeature, string name, PluginsModule pluginsModule, Action<IHttpContext> requestHandler) : base(pluginFeature, name, pluginsModule)
        {
            RequestHandler = requestHandler;
        }

        /// <summary>
        ///     Gets or sets the handler used to handle incoming requests to this endpoint
        /// </summary>
        public Action<IHttpContext> RequestHandler { get; }

        #region Overrides of PluginEndPoint

        /// <inheritdoc />
        internal override void ProcessRequest(IHttpContext context)
        {
            RequestHandler(context);
        }

        #endregion
    }
}
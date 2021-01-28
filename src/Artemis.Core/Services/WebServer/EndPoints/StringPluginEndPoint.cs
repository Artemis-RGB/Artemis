using System;
using System.IO;
using EmbedIO;

namespace Artemis.Core.Services
{
    /// <summary>
    ///     Represents a plugin web endpoint receiving an a <see cref="string" /> and returning a <see cref="string" /> or
    ///     <see langword="null" />.
    /// </summary>
    public class StringPluginEndPoint : PluginEndPoint
    {
        private readonly Action<string>? _requestHandler;
        private readonly Func<string, string?>? _responseRequestHandler;

        internal StringPluginEndPoint(PluginFeature pluginFeature, string name, PluginsModule pluginsModule, Action<string> requestHandler) : base(pluginFeature, name, pluginsModule)
        {
            _requestHandler = requestHandler;
        }

        internal StringPluginEndPoint(PluginFeature pluginFeature, string name, PluginsModule pluginsModule, Func<string, string?> requestHandler) : base(pluginFeature, name, pluginsModule)
        {
            _responseRequestHandler = requestHandler;
        }

        #region Overrides of PluginEndPoint

        /// <inheritdoc />
        internal override void ProcessRequest(IHttpContext context)
        {
            if (context.Request.HttpVerb != HttpVerbs.Post)
                throw HttpException.MethodNotAllowed("This end point only accepts POST calls");
            
            context.Response.ContentType = MimeType.PlainText;

            using TextReader reader = context.OpenRequestText();
            string? response;
            if (_requestHandler != null)
            {
                _requestHandler(reader.ReadToEnd());
                return;
            }
                
            else if (_responseRequestHandler != null)
                response = _responseRequestHandler(reader.ReadToEnd());
            else
                throw new ArtemisCoreException("String plugin end point has no request handler");

            using TextWriter writer = context.OpenResponseText();
            writer.Write(response);
        }

        #endregion
    }
}
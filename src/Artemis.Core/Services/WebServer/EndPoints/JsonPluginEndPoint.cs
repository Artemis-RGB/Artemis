using System;
using System.IO;
using EmbedIO;
using Newtonsoft.Json;

namespace Artemis.Core.Services
{
    /// <summary>
    ///     Represents a plugin web endpoint receiving an object of type <typeparamref name="T" /> and returning any
    ///     <see cref="object" /> or <see langword="null" />.
    ///     <para>Note: Both will be deserialized and serialized respectively using JSON.</para>
    /// </summary>
    public class JsonPluginEndPoint<T> : PluginEndPoint
    {
        private readonly Action<T>? _requestHandler;
        private readonly Func<T, object?>? _responseRequestHandler;

        internal JsonPluginEndPoint(PluginFeature pluginFeature, string name, PluginsModule pluginsModule, Action<T> requestHandler) : base(pluginFeature, name, pluginsModule)
        {
            _requestHandler = requestHandler;
            ThrowOnFail = true;
        }

        internal JsonPluginEndPoint(PluginFeature pluginFeature, string name, PluginsModule pluginsModule, Func<T, object?> responseRequestHandler) : base(pluginFeature, name, pluginsModule)
        {
            _responseRequestHandler = responseRequestHandler;
            ThrowOnFail = true;
        }

        /// <summary>
        ///     Whether or not the end point should throw an exception if deserializing the received JSON fails.
        ///     If set to <see langword="false" /> malformed JSON is silently ignored; if set to <see langword="true" /> malformed
        ///     JSON throws a <see cref="JsonException" />.
        /// </summary>
        public bool ThrowOnFail { get; set; }

        #region Overrides of PluginEndPoint

        /// <inheritdoc />
        internal override void ProcessRequest(IHttpContext context)
        {
            if (context.Request.HttpVerb != HttpVerbs.Post)
                throw HttpException.MethodNotAllowed("This end point only accepts POST calls");
            
            context.Response.ContentType = MimeType.Json;

            using TextReader reader = context.OpenRequestText();
            object? response = null;
            try
            {
                T deserialized = JsonConvert.DeserializeObject<T>(reader.ReadToEnd());

                if (_requestHandler != null)
                {
                    _requestHandler(deserialized);
                    return;
                }

                if (_responseRequestHandler != null)
                    response = _responseRequestHandler(deserialized);
                else
                    throw new ArtemisCoreException("JSON plugin end point has no request handler");
            }
            catch (JsonException)
            {
                if (ThrowOnFail)
                    throw;
            }

            using TextWriter writer = context.OpenResponseText();
            writer.Write(JsonConvert.SerializeObject(response));
        }

        #endregion
    }
}
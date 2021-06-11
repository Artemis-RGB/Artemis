using System;
using System.IO;
using System.Threading.Tasks;
using Artemis.Core.Modules;
using EmbedIO;
using Newtonsoft.Json;

namespace Artemis.Core.Services
{
    /// <summary>
    ///     Represents a plugin web endpoint receiving an object of type <typeparamref name="T" /> and returning any
    ///     <see cref="object" /> or <see langword="null" />.
    ///     <para>Note: Both will be deserialized and serialized respectively using JSON.</para>
    /// </summary>
    public class DataModelJsonPluginEndPoint<T> : PluginEndPoint where T : DataModel
    {
        private readonly Module<T> _module;

        internal DataModelJsonPluginEndPoint(Module<T> module, string name, PluginsModule pluginsModule) : base(module, name, pluginsModule)
        {
            _module = module ?? throw new ArgumentNullException(nameof(module));

            ThrowOnFail = true;
            Accepts = MimeType.Json;
        }

        /// <summary>
        ///     Whether or not the end point should throw an exception if deserializing the received JSON fails.
        ///     If set to <see langword="false" /> malformed JSON is silently ignored; if set to <see langword="true" /> malformed
        ///     JSON throws a <see cref="JsonException" />.
        /// </summary>
        public bool ThrowOnFail { get; set; }

        #region Overrides of PluginEndPoint

        /// <inheritdoc />
        protected override async Task ProcessRequest(IHttpContext context)
        {
            if (context.Request.HttpVerb != HttpVerbs.Post && context.Request.HttpVerb != HttpVerbs.Put)
                throw HttpException.MethodNotAllowed("This end point only accepts POST and PUT calls");

            context.Response.ContentType = MimeType.Json;

            using TextReader reader = context.OpenRequestText();
            try
            {
                JsonConvert.PopulateObject(await reader.ReadToEndAsync(), _module.DataModel);
            }
            catch (JsonException)
            {
                if (ThrowOnFail)
                    throw;
            }
        }

        #endregion
    }
}
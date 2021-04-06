using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmbedIO;

namespace Artemis.Core.Services
{
    /// <summary>
    ///     Represents an EmbedIO web module used to process web requests and forward them to the right
    ///     <see cref="PluginEndPoint" />.
    /// </summary>
    public class PluginsModule : WebModuleBase
    {
        private readonly Dictionary<string, Dictionary<string, PluginEndPoint>> _pluginEndPoints;

        internal PluginsModule(string baseRoute) : base(baseRoute)
        {
            _pluginEndPoints = new Dictionary<string, Dictionary<string, PluginEndPoint>>();
        }

        internal void AddPluginEndPoint(PluginEndPoint registration)
        {
            string id = registration.PluginFeature.Plugin.Guid.ToString();
            if (!_pluginEndPoints.TryGetValue(id, out Dictionary<string, PluginEndPoint>? registrations))
            {
                registrations = new Dictionary<string, PluginEndPoint>();
                _pluginEndPoints.Add(id, registrations);
            }

            if (registrations.ContainsKey(registration.Name))
                throw new ArtemisPluginException(registration.PluginFeature.Plugin, $"Plugin already registered an endpoint at {registration.Name}.");
            registrations.Add(registration.Name, registration);
        }

        internal void RemovePluginEndPoint(PluginEndPoint registration)
        {
            string id = registration.PluginFeature.Plugin.Guid.ToString();
            if (!_pluginEndPoints.TryGetValue(id, out Dictionary<string, PluginEndPoint>? registrations))
                return;
            if (!registrations.ContainsKey(registration.Name))
                return;
            registrations.Remove(registration.Name);
        }

        #region Overrides of WebModuleBase

        /// <inheritdoc />
        protected override async Task OnRequestAsync(IHttpContext context)
        {
            if (context.Route.SubPath == null)
                return;

            // Split the sub path
            string[] pathParts = context.Route.SubPath.Substring(1).Split('/');
            // Expect a plugin ID and an endpoint
            if (pathParts.Length != 2)
                return;

            // Find a matching plugin
            if (!_pluginEndPoints.TryGetValue(pathParts[0], out Dictionary<string, PluginEndPoint>? endPoints))
                throw HttpException.NotFound($"Found no plugin with ID {pathParts[0]}.");

            // Find a matching endpoint
            if (!endPoints.TryGetValue(pathParts[1], out PluginEndPoint? endPoint))
                throw HttpException.NotFound($"Found no endpoint called {pathParts[1]} for plugin with ID {pathParts[0]}.");

            // If Accept-Charset contains a wildcard, remove the header so we default to UTF8
            // This is a workaround for an EmbedIO ehh issue
            string? acceptCharset = context.Request.Headers["Accept-Charset"];
            if (acceptCharset != null && acceptCharset.Contains("*"))
                context.Request.Headers.Remove("Accept-Charset");

            // It is up to the registration how the request is eventually handled, it might even set a response here
            await endPoint.InternalProcessRequest(context);

            // No need to return ourselves, assume the request is fully handled by the end point
            context.SetHandled();
        }

        /// <inheritdoc />
        public override bool IsFinalHandler => false;

        internal string? ServerUrl { get; set; }

        /// <summary>
        ///     Gets a read only collection containing all current plugin end points
        /// </summary>
        public IReadOnlyCollection<PluginEndPoint> PluginEndPoints => new List<PluginEndPoint>(_pluginEndPoints.SelectMany(p => p.Value.Values));

        #endregion
    }
}
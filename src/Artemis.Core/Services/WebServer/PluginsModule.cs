using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using EmbedIO;
using Newtonsoft.Json;

namespace Artemis.Core.Services
{
    internal class PluginsModule : WebModuleBase
    {
        private readonly Dictionary<string, Dictionary<string, PluginEndPoint>> _pluginEndPoints;

        /// <inheritdoc />
        public PluginsModule(string baseRoute) : base(baseRoute)
        {
            _pluginEndPoints = new Dictionary<string, Dictionary<string, PluginEndPoint>>();
            OnUnhandledException += HandleUnhandledExceptionJson;
        }

        public void AddPluginEndPoint(PluginEndPoint registration)
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

        public void RemovePluginEndPoint(PluginEndPoint registration)
        {
            string id = registration.PluginFeature.Plugin.Guid.ToString();
            if (!_pluginEndPoints.TryGetValue(id, out Dictionary<string, PluginEndPoint>? registrations))
                return;
            if (!registrations.ContainsKey(registration.Name))
                return;
            registrations.Remove(registration.Name);
        }

        private async Task HandleUnhandledExceptionJson(IHttpContext context, Exception exception)
        {
            await context.SendStringAsync(
                JsonConvert.SerializeObject(new ArtemisPluginException("The plugin failed to process the request", exception), Formatting.Indented),
                MimeType.Json,
                Encoding.UTF8
            );
        }

        #region Overrides of WebModuleBase

        /// <inheritdoc />
        protected override async Task OnRequestAsync(IHttpContext context)
        {
            if (context.Route.SubPath == null)
                throw HttpException.NotFound();
            
            // Split the sub path
            string[] pathParts = context.Route.SubPath.Substring(1).Split('/');
            // Expect a plugin ID and an endpoint
            if (pathParts == null || pathParts.Length != 2)
                throw HttpException.BadRequest("Path must contain a plugin ID and endpoint and nothing else.");
            
            // Find a matching plugin
            if (!_pluginEndPoints.TryGetValue(pathParts[0], out Dictionary<string, PluginEndPoint>? endPoints))
                throw HttpException.NotFound($"Found no plugin with ID {pathParts[0]}.");
            
            // Find a matching endpoint
            if (!endPoints.TryGetValue(pathParts[1], out PluginEndPoint? endPoint))
                throw HttpException.NotFound($"Found no endpoint called {pathParts[1]} for plugin with ID {pathParts[0]}.");
            
            // It is up to the registration how the request is eventually handled, it might even set a response here
            endPoint.ProcessRequest(context);

            // No need to return ourselves, assume the request is fully handled by the end point
            context.SetHandled();
        }

        /// <inheritdoc />
        public override bool IsFinalHandler => true;

        #endregion
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;

namespace Artemis.Core.Services
{
    internal class PluginsController : WebApiController
    {
        private readonly IWebServerService _webServerService;

        public PluginsController(IWebServerService webServerService)
        {
            _webServerService = webServerService;
        }

        [Route(HttpVerbs.Get, "/plugins/endpoints")]
        public IReadOnlyCollection<PluginEndPoint> GetPluginEndPoints()
        {
            return _webServerService.PluginsModule.PluginEndPoints;
        }

        [Route(HttpVerbs.Get, "/plugins/endpoints/{plugin}/{endPoint}")]
        public PluginEndPoint GetPluginEndPoint(Guid plugin, string endPoint)
        {
            PluginEndPoint? pluginEndPoint = _webServerService.PluginsModule.PluginEndPoints.FirstOrDefault(e => e.PluginFeature.Plugin.Guid == plugin && e.Name == endPoint);
            if (pluginEndPoint == null)
                throw HttpException.NotFound();

            return pluginEndPoint;
        }
    }
}
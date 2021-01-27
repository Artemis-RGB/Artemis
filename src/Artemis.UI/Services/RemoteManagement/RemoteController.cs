using System;
using Artemis.Core.Services;
using Artemis.UI.Shared.Services;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;

namespace Artemis.UI.Services
{
    public class RemoteController : WebApiController
    {
        private readonly ICoreService _coreService;
        private readonly IWindowService _windowService;

        public RemoteController(ICoreService coreService, IWindowService windowService)
        {
            _coreService = coreService;
            _windowService = windowService;
        }

        [Route(HttpVerbs.Post, "/remote/bring-to-foreground")]
        public void PostBringToForeground()
        {
            _windowService.OpenMainWindow();
        }

        [Route(HttpVerbs.Post, "/remote/restart")]
        public void PostRestart()
        {
            Core.Utilities.Restart(_coreService.IsElevated, TimeSpan.FromMilliseconds(500));
        }

        [Route(HttpVerbs.Post, "/remote/shutdown")]
        public void PostShutdown()
        {
            Core.Utilities.Shutdown();
        }
    }
}
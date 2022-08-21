using System;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Shared.Services.MainWindow;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;

namespace Artemis.UI.Controllers;

public class RemoteController : WebApiController
{
    private readonly ICoreService _coreService;
    private readonly IMainWindowService _mainWindowService;

    public RemoteController(ICoreService coreService, IMainWindowService mainWindowService)
    {
        _coreService = coreService;
        _mainWindowService = mainWindowService;
    }

    [Route(HttpVerbs.Post, "/remote/bring-to-foreground")]
    public void PostBringToForeground()
    {
        _mainWindowService.OpenMainWindow();
    }

    [Route(HttpVerbs.Post, "/remote/restart")]
    public void PostRestart()
    {
        Utilities.Restart(_coreService.IsElevated, TimeSpan.FromMilliseconds(500));
    }

    [Route(HttpVerbs.Post, "/remote/shutdown")]
    public void PostShutdown()
    {
        Utilities.Shutdown();
    }
}
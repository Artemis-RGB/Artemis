using System;
using System.IO;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Shared.Routing;
using Artemis.UI.Shared.Services.MainWindow;
using Avalonia.Threading;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;

namespace Artemis.UI.Controllers;

public class RemoteController : WebApiController
{
    private readonly ICoreService _coreService;
    private readonly IMainWindowService _mainWindowService;
    private readonly IRouter _router;

    public RemoteController(ICoreService coreService, IMainWindowService mainWindowService, IRouter router)
    {
        _coreService = coreService;
        _mainWindowService = mainWindowService;
        _router = router;
    }

    [Route(HttpVerbs.Any, "/status")]
    public void GetStatus()
    {
        HttpContext.Response.StatusCode = 200;
    }

    [Route(HttpVerbs.Post, "/remote/bring-to-foreground")]
    public void PostBringToForeground()
    {
        using StreamReader reader = new(Request.InputStream);
        string route = reader.ReadToEnd();

        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            if (!string.IsNullOrWhiteSpace(route))
                await _router.Navigate(route);
            _mainWindowService.OpenMainWindow();
        });
    }

    [Route(HttpVerbs.Post, "/remote/restart")]
    public void PostRestart([FormField] string[] args)
    {
        Utilities.Restart(_coreService.IsElevated, TimeSpan.FromMilliseconds(500), args);
    }

    [Route(HttpVerbs.Post, "/remote/shutdown")]
    public void PostShutdown()
    {
        Utilities.Shutdown();
    }
}
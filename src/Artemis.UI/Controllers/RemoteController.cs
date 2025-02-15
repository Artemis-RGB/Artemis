using System;
using System.Collections.Generic;
using System.IO;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Shared.Routing;
using Artemis.UI.Shared.Services.MainWindow;
using Avalonia.Threading;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Webservices;

namespace Artemis.UI.Controllers;

public class RemoteController
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

    [ResourceMethod(RequestMethod.Post, "bring-to-foreground")]
    public void BringToForeground(IRequest request)
    {
        // Get the route from the request content stream
        // TODO: Use [FromBody] attribute instead once GenHTTP allows omitting null values
        string? route = null;
        if (request.Content != null)
        {
            using StreamReader reader = new(request.Content);
            route = reader.ReadToEnd();
        }

        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            if (!string.IsNullOrWhiteSpace(route))
                await _router.Navigate(route);
            _mainWindowService.OpenMainWindow();
        });
    }

    [ResourceMethod(RequestMethod.Post, "restart")]
    public void Restart(List<string> args)
    {
        Utilities.Restart(_coreService.IsElevated, TimeSpan.FromMilliseconds(500), args.ToArray());
    }

    [ResourceMethod(RequestMethod.Post, "shutdown")]
    public void Shutdown()
    {
        Utilities.Shutdown();
    }
}
using System;
using System.Threading;
using System.Threading.Tasks;
using DryIoc;
using Serilog;

namespace Artemis.UI.Shared.Routing;

internal class Navigation
{
    private readonly IContainer _container;
    private readonly ILogger _logger;

    private readonly IRoutableScreen _root;
    private readonly RouteResolution _resolution;
    private readonly RouterNavigationOptions _options;
    private CancellationTokenSource _cts;

    public Navigation(IContainer container, ILogger logger, IRoutableScreen root, RouteResolution resolution, RouterNavigationOptions options)
    {
        _container = container;
        _logger = logger;

        _root = root;
        _resolution = resolution;
        _options = options;
        _cts = new CancellationTokenSource();
    }

    public bool Cancelled => _cts.IsCancellationRequested;
    public bool Completed { get; private set; }

    public async Task Navigate(NavigationArguments args)
    {
        _logger.Information("Navigating to {Path}", _resolution.Path);
        _cts = new CancellationTokenSource();
        await NavigateResolution(_resolution, args, _root);

        if (!Cancelled)
            _logger.Information("Navigated to {Path}", _resolution.Path);
    }

    public void Cancel()
    {
        if (Cancelled || Completed)
            return;

        _logger.Information("Cancelled navigation to {Path}", _resolution.Path);
        _cts.Cancel();
    }

    private async Task NavigateResolution(RouteResolution resolution, NavigationArguments args, IRoutableScreen host)
    {
        if (Cancelled)
            return;

        // Reuse the screen if its type has not changed
        object screen;
        if (_options.RecycleScreens && host.RecycleScreen && host.InternalScreen != null && host.InternalScreen.GetType() == resolution.ViewModel)
            screen = host.InternalScreen;
        else
            screen = resolution.GetViewModel(_container);

        // If resolution has a child, ensure the screen can host it
        if (resolution.Child != null && screen is not IRoutableScreen)
            throw new ArtemisRoutingException($"Route resolved with a child but view model of type {resolution.ViewModel} is does mot implement {nameof(IRoutableScreen)}.");

        // Only change the screen if it wasn't reused
        if (!ReferenceEquals(host.InternalScreen, screen))
            host.InternalChangeScreen(screen);
        
        // TODO Decide whether it's a good idea to wait for the screen to activate

        if (CancelIfRequested(args, "ChangeScreen", screen))
            return;

        // If the screen implements some form of Navigable, activate it
        args.SegmentParameters = resolution.Parameters ?? Array.Empty<object>();
        if (screen is IRoutableScreen routableScreen)
        {
            try
            {
                await routableScreen.InternalOnNavigating(args, _cts.Token);
            }
            catch (Exception e)
            {
                Cancel();
                _logger.Error(e, "Failed to navigate to {Path}", resolution.Path);
            }
        }

        if (CancelIfRequested(args, "OnNavigating", screen))
            return;

        if (resolution.Child != null && screen is IRoutableScreen childScreen)
            await NavigateResolution(resolution.Child, args, childScreen);

        Completed = true;
    }

    public bool PathEquals(string path, bool allowPartialMatch)
    {
        if (allowPartialMatch)
            return _resolution.Path.StartsWith(path, StringComparison.InvariantCultureIgnoreCase);
        return string.Equals(_resolution.Path, path, StringComparison.InvariantCultureIgnoreCase);
    }

    private bool CancelIfRequested(NavigationArguments args, string stage, object screen)
    {
        if (Cancelled)
            return true;

        if (!args.Cancelled)
            return false;

        _logger.Debug("Navigation to {Path} during {Stage} by {Screen}", args.Path, stage, screen.GetType().Name);
        Cancel();
        return true;
    }
}
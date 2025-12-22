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

    private readonly IRoutableHostScreen _root;
    private readonly RouteResolution _resolution;
    private readonly RouterNavigationOptions _options;
    private CancellationTokenSource _cts;

    public Navigation(IContainer container, ILogger logger, IRoutableHostScreen root, RouteResolution resolution, RouterNavigationOptions options)
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
        if (_options.EnableLogging)
            _logger.Information("Navigating to {Path}", _resolution.Path);
        _cts = new CancellationTokenSource();
        await NavigateResolution(_resolution, args, _root);

        if (!Cancelled && _options.EnableLogging)
            _logger.Information("Navigated to {Path}", _resolution.Path);
    }

    public void Cancel()
    {
        if (Cancelled || Completed)
            return;

        if (_options.EnableLogging)
            _logger.Information("Cancelled navigation to {Path}", _resolution.Path);
        _cts.Cancel();
    }

    private async Task NavigateResolution(RouteResolution resolution, NavigationArguments args, IRoutableHostScreen host)
    {
        if (Cancelled)
            return;

        // Reuse the screen if its type has not changed, if a new one must be created, don't do so on the UI thread
        IRoutableScreen screen;
        if (_options.RecycleScreens && host.RecycleScreen && host.InternalScreen != null && host.InternalScreen.GetType() == resolution.ViewModel)
            screen = host.InternalScreen;
        else
            screen = await Task.Run(() => resolution.GetViewModel(_container));

        // If resolution has a child, ensure the screen can host it
        if (resolution.Child != null && screen is not IRoutableHostScreen)
            throw new ArtemisRoutingException($"Route resolved with a child but view model of type {resolution.ViewModel} is does mot implement {nameof(IRoutableHostScreen)}.");

        // Only change the screen if it wasn't reused
        if (!ReferenceEquals(host.InternalScreen, screen))
        {
            try
            {
                host.InternalChangeScreen(screen);
            }
            catch (Exception e)
            {
                Cancel();
                if (e is not TaskCanceledException)
                    _logger.Error(e, "Failed to navigate to {Path}", resolution.Path);
            }
        }

        if (CancelIfRequested(args, "ChangeScreen", screen))
            return;
        
        // Navigate on the screen
        args.SegmentParameters = resolution.Parameters ?? [];
        try
        {
            await screen.InternalOnNavigating(args, _cts.Token);
        }
        catch (Exception e)
        {
            Cancel();
            if (e is not TaskCanceledException)
                _logger.Error(e, "Failed to navigate to {Path}", resolution.Path);
        }

        if (CancelIfRequested(args, "OnNavigating", screen))
            return;

        if (screen is IRoutableHostScreen childScreen)
        {
            // Navigate the child too
            if (resolution.Child != null)
                await NavigateResolution(resolution.Child, args, childScreen);
            // Without a resolution, navigate to the default screen (which may be null)
            else if (childScreen.InternalScreen != childScreen.InternalDefaultScreen)
                childScreen.InternalChangeScreen(childScreen.InternalDefaultScreen);
        }

        Completed = true;
    }

    public bool PathEquals(string path, RouterNavigationOptions options)
    {
        return options.PathEquals(_resolution.Path, path);
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
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DryIoc;
using Serilog;

namespace Artemis.UI.Shared.Routing;

internal class Navigation
{
    private readonly IContainer _container;
    private readonly ILogger _logger;

    private readonly IRoutable _root;
    private readonly RouteResolution _resolution;
    private readonly RouterNavigationOptions _options;
    private CancellationTokenSource _cts;

    public Navigation(IContainer container, ILogger logger, IRoutable root, RouteResolution resolution, RouterNavigationOptions options)
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

    public async Task Navigate()
    {
        _logger.Information("Navigating to {Path}", _resolution.Path);
        _cts = new CancellationTokenSource();
        NavigationArguments args = new(_resolution.Path, _resolution.GetAllParameters());
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

    private async Task NavigateResolution(RouteResolution resolution, NavigationArguments args, IRoutable host)
    {
        if (Cancelled)
            return;

        // Reuse the screen if its type has not changed
        object screen;
        if (_options.RecycleScreens && host.RecycleScreen && host.Screen != null && host.Screen.GetType() == resolution.ViewModel)
            screen = host.Screen;
        else
            screen = resolution.GetViewModel(_container);

        if (resolution.Child != null && screen is not IRoutable)
            throw new ArtemisRoutingException($"Route resolved with a child but view model of type {resolution.ViewModel} is does mot implement {nameof(IRoutable)}.");

        // Only change the screen if it wasn't reused
        if (!ReferenceEquals(host.Screen, screen))
            host.ChangeScreen(screen);

        // The screen change may have triggered a cancel
        if (Cancelled)
            return;

        // If the screen implements some form of Navigable, activate it
        args.SegmentParameters = resolution.Parameters ?? Array.Empty<object>();
        if (screen is INavigable navigable)
        {
            try
            {
                await navigable.Navigated(args, _cts.Token);
            }
            catch (Exception e)
            {
                Cancel();
                _logger.Error(e, "Failed to navigate to {Path}", resolution.Path);
            }
        }

        // The Activate may cancel via the args, in that case apply the cancellation
        if (args.Cancelled)
        {
            _logger.Debug("Navigation to {Path} cancelled by {Screen}", resolution.Path, screen.GetType().Name);
            Cancel();
            return;
        }

        if (resolution.Child != null && screen is IRoutable childHost)
            await NavigateResolution(resolution.Child, args, childHost);

        Completed = true;
    }

    public bool PathEquals(string path, bool allowPartialMatch)
    {
        if (allowPartialMatch)
            return _resolution.Path.StartsWith(path, StringComparison.InvariantCultureIgnoreCase);
        return string.Equals(_resolution.Path, path, StringComparison.InvariantCultureIgnoreCase);
    }
}
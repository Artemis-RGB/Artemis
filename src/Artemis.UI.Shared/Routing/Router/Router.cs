using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Artemis.Core;
using Serilog;

namespace Artemis.UI.Shared.Routing;

internal class Router : CorePropertyChanged, IRouter
{
    private readonly Stack<string> _backStack = new();
    private readonly BehaviorSubject<string?> _currentRouteSubject;
    private readonly Stack<string> _forwardStack = new();
    private readonly Func<IRoutableScreen, RouteResolution, RouterNavigationOptions, Navigation> _getNavigation;
    private readonly ILogger _logger;
    private Navigation? _currentNavigation;

    private IRoutableScreen? _root;

    public Router(ILogger logger, Func<IRoutableScreen, RouteResolution, RouterNavigationOptions, Navigation> getNavigation)
    {
        _logger = logger;
        _getNavigation = getNavigation;
        _currentRouteSubject = new BehaviorSubject<string?>(null);
    }

    private async Task<RouteResolution> Resolve(string path)
    {
        foreach (IRouterRegistration routerRegistration in Routes)
        {
            RouteResolution result = await RouteResolution.Resolve(routerRegistration, path);
            if (result.Success)
                return result;
        }

        return RouteResolution.AsFailure(path);
    }

    private async Task<bool> RequestClose(object screen, NavigationArguments args)
    {
        if (screen is not IRoutableScreen routableScreen)
            return true;

        await routableScreen.InternalOnClosing(args);
        if (args.Cancelled)
        {
            _logger.Debug("Navigation to {Path} cancelled during RequestClose by {Screen}", args.Path, screen.GetType().Name);
            return false;
        }

        if (routableScreen.InternalScreen == null)
            return true;
        return await RequestClose(routableScreen.InternalScreen, args);
    }

    private bool PathEquals(string path, bool allowPartialMatch)
    {
        if (allowPartialMatch)
            return _currentRouteSubject.Value != null && _currentRouteSubject.Value.StartsWith(path, StringComparison.InvariantCultureIgnoreCase);
        return string.Equals(_currentRouteSubject.Value, path, StringComparison.InvariantCultureIgnoreCase);
    }

    /// <inheritdoc />
    public IObservable<string?> CurrentPath => _currentRouteSubject;

    /// <inheritdoc />
    public List<IRouterRegistration> Routes { get; } = new();

    /// <inheritdoc />
    public async Task Navigate(string path, RouterNavigationOptions? options = null)
    {
        options ??= new RouterNavigationOptions();

        if (_root == null)
            throw new ArtemisRoutingException("Cannot navigate without a root having been set");
        if (PathEquals(path, options.IgnoreOnPartialMatch) || (_currentNavigation != null && _currentNavigation.PathEquals(path, options.IgnoreOnPartialMatch)))
            return;

        RouteResolution resolution = await Resolve(path);
        if (!resolution.Success)
        {
            _logger.Warning("Failed to resolve path {Path}", path);
            return;
        }

        NavigationArguments args = new(resolution.Path, resolution.GetAllParameters());

        if (!await RequestClose(_root, args))
            return;

        Navigation navigation = _getNavigation(_root, resolution, options);

        _currentNavigation?.Cancel();
        _currentNavigation = navigation;

        // Execute the navigation
        await navigation.Navigate(args);

        // If it was cancelled before completion, don't add it to history or update the current path
        if (navigation.Cancelled)
            return;

        _currentRouteSubject.OnNext(path);
        if (options.AddToHistory)
        {
            _backStack.Push(path);
            _forwardStack.Clear();
        }
    }

    /// <inheritdoc />
    public async Task<bool> GoBack()
    {
        if (!_backStack.TryPop(out string? path))
            return false;

        await Navigate(path);
        _forwardStack.Push(path);
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> GoForward()
    {
        if (!_forwardStack.TryPop(out string? path))
            return false;

        await Navigate(path);
        _backStack.Push(path);
        return true;
    }

    /// <inheritdoc />
    public void ClearHistory()
    {
        _backStack.Clear();
        _forwardStack.Clear();
    }

    /// <inheritdoc />
    public void SetRoot<TScreen>(RoutableScreen<TScreen> root) where TScreen : class
    {
        _root = root;
    }

    /// <inheritdoc />
    public void SetRoot<TScreen, TParam>(RoutableScreen<TScreen, TParam> root) where TScreen : class where TParam : new()
    {
        _root = root;
    }
}
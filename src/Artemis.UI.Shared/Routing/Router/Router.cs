using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.UI.Shared.Services.MainWindow;
using Avalonia.Threading;
using Serilog;

namespace Artemis.UI.Shared.Routing;

internal class Router : CorePropertyChanged, IRouter, IDisposable
{
    private readonly Stack<string> _backStack = new();
    private readonly BehaviorSubject<string?> _currentRouteSubject;
    private readonly Stack<string> _forwardStack = new();
    private readonly Func<IRoutableHostScreen, RouteResolution, RouterNavigationOptions, Navigation> _getNavigation;
    private readonly ILogger _logger;
    private readonly IMainWindowService _mainWindowService;
    private Navigation? _currentNavigation;

    private IRoutableHostScreen? _root;
    private string? _previousWindowRoute;

    public Router(ILogger logger, IMainWindowService mainWindowService, Func<IRoutableHostScreen, RouteResolution, RouterNavigationOptions, Navigation> getNavigation)
    {
        _logger = logger;
        _mainWindowService = mainWindowService;
        _getNavigation = getNavigation;
        _currentRouteSubject = new BehaviorSubject<string?>(null);

        mainWindowService.MainWindowOpened += MainWindowServiceOnMainWindowOpened;
        mainWindowService.MainWindowClosed += MainWindowServiceOnMainWindowClosed;
    }

    private RouteResolution Resolve(string path)
    {
        foreach (IRouterRegistration routerRegistration in Routes)
        {
            RouteResolution result = RouteResolution.Resolve(routerRegistration, path);
            if (result.Success)
                return result;
        }

        return RouteResolution.AsFailure(path);
    }

    private async Task<bool> RequestClose(IRoutableScreen screen, NavigationArguments args)
    {
        // Drill down to child screens first
        if (screen is IRoutableHostScreen hostScreen && hostScreen.InternalScreen != null && !await RequestClose(hostScreen.InternalScreen, args))
            return false;

        await screen.InternalOnClosing(args);
        if (!args.Cancelled)
            return true;
        _logger.Debug("Navigation to {Path} cancelled during RequestClose by {Screen}", args.Path, screen.GetType().Name);
        return false;
    }

    private bool PathEquals(string path, RouterNavigationOptions options)
    {
        return _currentRouteSubject.Value != null && options.PathEquals(_currentRouteSubject.Value, path);
    }

    /// <inheritdoc />
    public IObservable<string?> CurrentPath => _currentRouteSubject;

    /// <inheritdoc />
    public List<IRouterRegistration> Routes { get; } = new();

    /// <inheritdoc />
    public async Task Navigate(string path, RouterNavigationOptions? options = null)
    {
        if (path.StartsWith('/') && _currentRouteSubject.Value != null)
            path = _currentRouteSubject.Value + path;
        if (path.StartsWith("../") && _currentRouteSubject.Value != null)
            path = NavigateUp(_currentRouteSubject.Value, path);
        else
            path = path.ToLower().Trim(' ', '/', '\\');

        options ??= new RouterNavigationOptions();

        // Routing takes place on the UI thread with processing heavy tasks offloaded by the router itself
        await Dispatcher.UIThread.InvokeAsync(() => InternalNavigate(path, options));
    }

    /// <inheritdoc />
    public async Task Reload()
    {
        string path = _currentRouteSubject.Value ?? "blank";

        // Routing takes place on the UI thread with processing heavy tasks offloaded by the router itself
        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            await InternalNavigate("blank", new RouterNavigationOptions {AddToHistory = false, RecycleScreens = false, EnableLogging = false});
            await InternalNavigate(path, new RouterNavigationOptions {AddToHistory = false, RecycleScreens = false});
        });
    }

    private async Task InternalNavigate(string path, RouterNavigationOptions options)
    {
        if (_root == null)
            throw new ArtemisRoutingException("Cannot navigate without a root having been set");

        // If navigating to the same path, don't do anything
        if ((_currentNavigation == null && PathEquals(path, options)) || (_currentNavigation != null && _currentNavigation.PathEquals(path, options)))
            return;

        string? previousPath = _currentRouteSubject.Value;
        RouteResolution resolution = Resolve(path);
        if (!resolution.Success)
        {
            _logger.Warning("Failed to resolve path {Path}", path);
            return;
        }

        NavigationArguments args = new(this, options, resolution.Path, resolution.GetAllParameters());

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

        if (options.AddToHistory && previousPath != null)
        {
            _backStack.Push(previousPath);
            _forwardStack.Clear();
        }

        _currentRouteSubject.OnNext(path);
    }

    /// <inheritdoc />
    public async Task<bool> GoBack()
    {
        if (!_backStack.TryPop(out string? path))
            return false;

        string? previousPath = _currentRouteSubject.Value;
        await Navigate(path, new RouterNavigationOptions {AddToHistory = false});
        if (previousPath != null)
            _forwardStack.Push(previousPath);

        return true;
    }

    /// <inheritdoc />
    public async Task<bool> GoForward()
    {
        if (!_forwardStack.TryPop(out string? path))
            return false;

        string? previousPath = _currentRouteSubject.Value;
        await Navigate(path, new RouterNavigationOptions {AddToHistory = false});
        if (previousPath != null)
            _backStack.Push(previousPath);

        return true;
    }

    /// <inheritdoc />
    public async Task<bool> GoUp(RouterNavigationOptions? options = null)
    {
        string? currentPath = _currentRouteSubject.Value;

        // Keep removing segments until we find a parent route that resolves
        while (currentPath != null && currentPath.Contains('/'))
        {
            string parentPath = currentPath[..currentPath.LastIndexOf('/')];
            RouteResolution resolution = Resolve(parentPath);
            if (resolution.Success)
            {
                await Navigate(parentPath, options);
                return true;
            }

            currentPath = parentPath;
        }

        return false;
    }

    /// <inheritdoc />
    public void ClearHistory()
    {
        _backStack.Clear();
        _forwardStack.Clear();
    }

    /// <inheritdoc />
    public void SetRoot<TScreen>(RoutableHostScreen<TScreen> root) where TScreen : RoutableScreen
    {
        _root = root;
    }

    /// <inheritdoc />
    public void SetRoot<TScreen, TParam>(RoutableHostScreen<TScreen, TParam> root) where TScreen : RoutableScreen where TParam : new()
    {
        _root = root;
    }

    /// <inheritdoc />
    public void ClearPreviousWindowRoute()
    {
        _previousWindowRoute = null;
    }

    public void Dispose()
    {
        _currentRouteSubject.Dispose();
        _mainWindowService.MainWindowOpened -= MainWindowServiceOnMainWindowOpened;
        _mainWindowService.MainWindowClosed -= MainWindowServiceOnMainWindowClosed;

        _logger.Debug("Router disposed, should that be? Stacktrace: \r\n{StackTrace}", Environment.StackTrace);
    }


    private string NavigateUp(string current, string path)
    {
        string[] pathParts = current.Split('/');
        string[] navigateParts = path.Split('/');
        int upCount = navigateParts.TakeWhile(part => part == "..").Count();

        if (upCount >= pathParts.Length)
        {
            throw new InvalidOperationException("Cannot navigate up beyond the root");
        }

        IEnumerable<string> remainingCurrentPathParts = pathParts.Take(pathParts.Length - upCount);
        IEnumerable<string> remainingNavigatePathParts = navigateParts.Skip(upCount);

        return string.Join("/", remainingCurrentPathParts.Concat(remainingNavigatePathParts));
    }

    private void MainWindowServiceOnMainWindowOpened(object? sender, EventArgs e)
    {
        if (_previousWindowRoute != null && _currentRouteSubject.Value == "blank")
            Dispatcher.UIThread.InvokeAsync(async () => await Navigate(_previousWindowRoute, new RouterNavigationOptions {AddToHistory = false, EnableLogging = false}));
        else if (_currentRouteSubject.Value == null || _currentRouteSubject.Value == "blank")
        {
            string? startupRoute = Constants.GetStartupRoute();
            if (startupRoute != null)
                Dispatcher.UIThread.InvokeAsync(async () => await Navigate(startupRoute, new RouterNavigationOptions {AddToHistory = false, EnableLogging = true}));
            else
                Dispatcher.UIThread.InvokeAsync(async () => await Navigate("home", new RouterNavigationOptions {AddToHistory = false, EnableLogging = true}));
        }
    }

    private void MainWindowServiceOnMainWindowClosed(object? sender, EventArgs e)
    {
        _previousWindowRoute = _currentRouteSubject.Value;
        Dispatcher.UIThread.InvokeAsync(async () => await Navigate("blank", new RouterNavigationOptions {AddToHistory = false, EnableLogging = false}));
    }
}
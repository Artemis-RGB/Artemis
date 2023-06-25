using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Artemis.Core;
using DryIoc;

namespace Artemis.UI.Shared.Routing;

public class Router : CorePropertyChanged, IRouter
{
    private readonly IContainer _container;
    private readonly Stack<string> _backStack = new();
    private readonly Stack<string> _forwardStack = new();

    private IRoutable? _root;
    private string? _currentPath;

    public Router(IContainer container)
    {
        _container = container;
    }

    public IRoutable? Root
    {
        get => _root;
        set => SetAndNotify(ref _root, value);
    }

    public string? CurrentPath
    {
        get => _currentPath;
        private set => SetAndNotify(ref _currentPath, value);
    }

    public List<IRouterRegistration> Routes { get; } = new();

    public async Task<bool> Navigate(string path, RouterNavigationOptions? options = null)
    {
        if (Root == null)
            throw new ArtemisRoutingException($"Cannot navigate without a root having been set");

        RouteResolution resolution = await Resolve(path);
        if (!resolution.Success)
            return false;

        options ??= new RouterNavigationOptions();

        try
        {
            bool success = await Root.Activate(resolution, _container);
            if (success)
                CurrentPath = path;

            if (options.AddToHistory)
            {
                _backStack.Push(path);
                _forwardStack.Clear();
            }

            return success;
        }
        catch (Exception e)
        {
            throw new ArtemisRoutingException($"Failed to navigate to {path}", e);
        }
    }

    public async Task<bool> GoBack()
    {
        if (!_backStack.TryPop(out string? path))
            return false;

        bool success = await Navigate(path);
        if (success)
            _forwardStack.Push(path);
        return success;
    }

    public async Task<bool> GoForward()
    {
        if (!_forwardStack.TryPop(out string? path))
            return false;

        bool success = await Navigate(path);
        if (success)
            _backStack.Push(path);
        return success;
    }

    public void ClearHistory()
    {
        _backStack.Clear();
        _forwardStack.Clear();
    }

    private async Task<RouteResolution> Resolve(string path)
    {
        foreach (IRouterRegistration routerRegistration in Routes)
        {
            RouteResolution result = await routerRegistration.Resolve(path);
            if (result.Success)
                return result;
        }

        return RouteResolution.AsFailure();
    }
}
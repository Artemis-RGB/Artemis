using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Reflection;
using System.Threading.Tasks;
using Artemis.Core;
using DryIoc;

namespace Artemis.UI.Shared.Routing;

internal class Router : CorePropertyChanged, IRouter
{
    private readonly Stack<string> _backStack = new();
    private readonly IContainer _container;
    private readonly BehaviorSubject<string?> _currentRouteSubject;
    private readonly Stack<string> _forwardStack = new();

    private IRoutable? _root;

    public Router(IContainer container)
    {
        _container = container;
        _currentRouteSubject = new BehaviorSubject<string?>(null);
    }

    public void ClearHistory()
    {
        _backStack.Clear();
        _forwardStack.Clear();
    }

    private async Task NavigateResolution(RouteResolution resolution, IRoutable host)
    {
        // Reuse the screen if its type has not changed
        object screen;
        if (host.Screen != null && host.Screen.GetType() == resolution.ViewModel)
            screen = host.Screen;
        else
            screen = resolution.GetViewModel(_container);

        if (resolution.Child != null)
            if (screen is not IRoutable)
                throw new ArtemisRoutingException($"Route resolved with a child but view model of type {resolution.ViewModel} is does mot implement {nameof(IRoutable)}.");

        // Only change the screen if it wasn't reused
        if (!ReferenceEquals(host.Screen, screen))
            host.ChangeScreen(screen);

        // If the screen implements some form of Navigable, activate it
        await ActivateNavigable(screen, resolution);

        if (resolution.Child != null && screen is IRoutable childHost)
            await NavigateResolution(resolution.Child, childHost);
    }

    private async Task ActivateNavigable(object screen, RouteResolution resolution)
    {
        if (screen is INavigable navigable)
        {
            await navigable.Navigated();
            return;
        }

        // Without parameters INavigable<TParameter> cannot be activated
        if (resolution.Parameters == null)
            return;

        // Kinda nasty to rely on reflection here but routing should not happen that often
        // If needed this could use cached expression trees

        // Ensure the screen is of type INavigable<TParameter>
        Type screenType = screen.GetType();
        Type navigableInterfaceType = typeof(INavigable<>);
        if (screenType.GetGenericArguments().Length != 1)
            return;

        Type navigableGenericType = navigableInterfaceType.MakeGenericType(screenType.GetGenericArguments());
        if (navigableGenericType.IsAssignableFrom(screenType))
        {
            // Create an instance of the parameter type, this assumes the parameter has a public constructor matching the parameters
            Type parameterType = screenType.GetGenericArguments()[0];
            object? parameter = Activator.CreateInstance(parameterType, resolution.Parameters);
            // Invoke the activate method 
            MethodInfo? method = screenType.GetMethod(nameof(INavigable<object>.Navigated));
            if (method != null && parameter != null)
                await (Task) method.Invoke(screen, new[] {parameter})!;
        }
    }

    private async Task<RouteResolution> Resolve(string path)
    {
        foreach (IRouterRegistration routerRegistration in Routes)
        {
            RouteResolution result = await routerRegistration.Resolve(path);
            if (result.Success)
                return result;
        }

        return RouteResolution.AsFailure(path);
    }

    /// <inheritdoc />
    public IRoutable? Root
    {
        get => _root;
        set => SetAndNotify(ref _root, value);
    }

    /// <inheritdoc />
    public IObservable<string?> CurrentPath => _currentRouteSubject;

    /// <inheritdoc />
    public List<IRouterRegistration> Routes { get; } = new();

    public async Task Navigate(string path, RouterNavigationOptions? options = null)
    {
        string? startPath = _currentRouteSubject.Value;

        if (Root == null)
            throw new ArtemisRoutingException("Cannot navigate without a root having been set");

        if (_currentRouteSubject.Value != null && _currentRouteSubject.Value.Equals(path, StringComparison.InvariantCultureIgnoreCase))
            return;

        RouteResolution resolution = await Resolve(path);
        if (!resolution.Success)
            return;

        options ??= new RouterNavigationOptions();

        try
        {
            // Update the path and stack before navigating, this ensures that if navigation triggers a redirect CurrentRoute and the stacks stay correct
            _currentRouteSubject.OnNext(path);
            if (options.AddToHistory)
            {
                _backStack.Push(path);
                _forwardStack.Clear();
            }

            await NavigateResolution(resolution, Root);
        }
        catch (Exception e)
        {
            // Put back the previous path
            if (_currentRouteSubject.Value != startPath)
                _currentRouteSubject.OnNext(startPath);

            // Clear the history to get rid of any broken state
            ClearHistory();

            throw new ArtemisRoutingException($"Failed to navigate to {path}", e);
        }
    }

    public async Task<bool> GoBack()
    {
        if (!_backStack.TryPop(out string? path))
            return false;

        await Navigate(path);
        _forwardStack.Push(path);
        return true;
    }

    public async Task<bool> GoForward()
    {
        if (!_forwardStack.TryPop(out string? path))
            return false;

        await Navigate(path);
        _backStack.Push(path);
        return true;
    }
}
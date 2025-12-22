using System;
using System.Collections.Generic;
using System.Linq;
using DryIoc;

namespace Artemis.UI.Shared.Routing;

internal class RouteResolution
{
    private RouteResolution(string path)
    {
        Path = path;
    }

    public string Path { get; }
    public bool Success { get; private set; }
    public Type? ViewModel { get; private set; }
    public object[]? Parameters { get; private set; }
    public RouteResolution? Child { get; private set; }

    public static RouteResolution Resolve(IRouterRegistration registration, string path)
    {
        List<string> segments = path.Split('/').ToList();
        if (registration.Route.Segments.Count > segments.Count)
            return AsFailure(path);

        // Ensure self is a match
        List<object> parameters = [];
        int currentSegment = 0;
        foreach (RouteSegment routeSegment in registration.Route.Segments)
        {
            string segment = segments[currentSegment];
            if (!routeSegment.IsMatch(segment))
                return AsFailure(path);

            object? parameter = routeSegment.GetParameter(segment);
            if (parameter != null)
                parameters.Add(parameter);

            currentSegment++;
        }

        if (currentSegment == segments.Count)
            return AsSuccess(registration.ViewModel, path, parameters.ToArray());

        // If segments remain, a child should match it
        string childPath = string.Join('/', segments.Skip(currentSegment));
        foreach (IRouterRegistration routerRegistration in registration.Children)
        {
            RouteResolution result = Resolve(routerRegistration, childPath);
            if (result.Success)
                return AsSuccess(registration.ViewModel, path, parameters.ToArray(), result);
        }

        return AsFailure(path);
    }

    public static RouteResolution AsFailure(string path)
    {
        return new RouteResolution(path);
    }

    public static RouteResolution AsSuccess(Type viewModel, string path, object[] parameters, RouteResolution? child = null)
    {
        if (child != null && !child.Success)
            throw new ArtemisRoutingException("Cannot create a success route resolution with a failed child");

        return new RouteResolution(path)
        {
            Success = true,
            ViewModel = viewModel,
            Parameters = parameters,
            Child = child
        };
    }

    public RoutableScreen GetViewModel(IContainer container)
    {
        return GetViewModel<RoutableScreen>(container);
    }

    public T GetViewModel<T>(IContainer container) where T : RoutableScreen
    {
        if (ViewModel == null)
            throw new ArtemisRoutingException("Cannot get a view model of a non-success route resolution");

        object? viewModel = container.Resolve(ViewModel);
        if (viewModel == null)
            throw new ArtemisRoutingException($"Could not resolve view model of type {ViewModel}");
        if (viewModel is not T typedViewModel)
            throw new ArtemisRoutingException($"View model of type {ViewModel} is does mot implement {typeof(T)}.");

        return typedViewModel;
    }

    public object[] GetAllParameters()
    {
        List<object> result = [];
        if (Parameters != null)
            result.AddRange(Parameters);
        object[]? childParameters = Child?.GetAllParameters();
        if (childParameters != null)
            result.AddRange(childParameters);

        return result.ToArray();
    }
}
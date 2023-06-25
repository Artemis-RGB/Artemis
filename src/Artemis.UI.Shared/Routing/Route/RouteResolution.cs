using System;
using DryIoc;

namespace Artemis.UI.Shared.Routing;

public class RouteResolution
{
    private RouteResolution()
    {
    }

    public bool Success { get; private set; }
    public string Path { get; set; }
    public Type? ViewModel { get; private set; }
    public object[]? Parameters { get; private set; }
    public RouteResolution? Child { get; private set; }

    public static RouteResolution AsFailure(string path)
    {
        return new RouteResolution
        {
            Path = path
        };
    }

    public static RouteResolution AsSuccess(Type viewModel, string path, object[] parameters, RouteResolution? child = null)
    {
        if (child != null && !child.Success)
            throw new ArtemisRoutingException("Cannot create a success route resolution with a failed child");

        return new RouteResolution
        {
            Success = true,
            Path = path,
            ViewModel = viewModel,
            Parameters = parameters,
            Child = child
        };
    }

    public object GetViewModel(IContainer container)
    {
        if (ViewModel == null)
            throw new ArtemisRoutingException("Cannot get a view model of a non-success route resolution");

        object? viewModel = container.Resolve(ViewModel);
        if (viewModel == null)
            throw new ArtemisRoutingException($"Could not resolve view model of type {ViewModel}");

        return viewModel;
    }

    public T GetViewModel<T>(IContainer container)
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
}
using System;

namespace Artemis.Core.Services;

/// <summary>
/// Represents a web API controller registration.
/// </summary>
/// <typeparam name="T">The type of the web API controller.</typeparam>
public class WebApiControllerRegistration<T> : WebApiControllerRegistration where T : class
{
    internal WebApiControllerRegistration(IWebServerService webServerService, PluginFeature feature, string path) : base(webServerService, feature, typeof(T), path)
    {
        Factory = () => feature.Plugin.Resolve<T>();
    }
}

/// <summary>
/// Represents a web API controller registration.
/// </summary>
public abstract class WebApiControllerRegistration
{
    private readonly IWebServerService _webServerService;

    /// <summary>
    /// Creates a new instance of the <see cref="WebApiControllerRegistration"/> class.
    /// </summary>
    protected internal WebApiControllerRegistration(IWebServerService webServerService, PluginFeature feature, Type controllerType, string path)
    {
        _webServerService = webServerService;
        Feature = feature;
        ControllerType = controllerType;
        Path = path;

        Feature.Disabled += FeatureOnDisabled;
    }

    private void FeatureOnDisabled(object? sender, EventArgs e)
    {
        _webServerService.RemoveController(this);
        Feature.Disabled -= FeatureOnDisabled;
    }

    /// <summary>
    /// Gets the type of the web API controller.
    /// </summary>
    public Type ControllerType { get; }

    /// <summary>
    /// Gets the plugin feature that provided the web API controller.
    /// </summary>
    public PluginFeature Feature { get; }

    /// <summary>
    /// Gets the path at which the controller is available.
    /// </summary>
    public string Path { get; }

    internal Func<object> Factory { get; set; }
}
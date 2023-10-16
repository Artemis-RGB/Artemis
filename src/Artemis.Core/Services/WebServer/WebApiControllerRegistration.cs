using System;
using EmbedIO.WebApi;

namespace Artemis.Core.Services;

/// <summary>
/// Represents a web API controller registration.
/// </summary>
/// <typeparam name="T">The type of the web API controller.</typeparam>
public class WebApiControllerRegistration<T> : WebApiControllerRegistration where T : WebApiController
{
    internal WebApiControllerRegistration(IWebServerService webServerService, PluginFeature feature) : base(webServerService, feature, typeof(T))
    {
        Factory = () => feature.Plugin.Resolve<T>();
    }

    internal Func<T> Factory { get; set; }
    internal override object UntypedFactory => Factory;
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
    protected internal WebApiControllerRegistration(IWebServerService webServerService, PluginFeature feature, Type controllerType)
    {
        _webServerService = webServerService;
        Feature = feature;
        ControllerType = controllerType;
        
        Feature.Disabled += FeatureOnDisabled;
    }

    private void FeatureOnDisabled(object? sender, EventArgs e)
    {
        _webServerService.RemoveController(this);
        Feature.Disabled -= FeatureOnDisabled;
    }

    internal abstract object UntypedFactory { get; }
    
    /// <summary>
    /// Gets the type of the web API controller.
    /// </summary>
    public Type ControllerType { get; }
    
    /// <summary>
    /// Gets the plugin feature that provided the web API controller.
    /// </summary>
    public PluginFeature Feature { get; }
}
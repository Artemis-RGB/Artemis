using System;
using EmbedIO;

namespace Artemis.Core.Services;

/// <summary>
/// Represents a registration for a web module.
/// </summary>
public class WebModuleRegistration
{
    private readonly IWebServerService _webServerService;

    internal WebModuleRegistration(IWebServerService webServerService, PluginFeature feature, Type webModuleType)
    {
        _webServerService = webServerService;
        Feature = feature ?? throw new ArgumentNullException(nameof(feature));
        WebModuleType = webModuleType ?? throw new ArgumentNullException(nameof(webModuleType));

        Feature.Disabled += FeatureOnDisabled;
    }

    internal WebModuleRegistration(IWebServerService webServerService, PluginFeature feature, Func<IWebModule> create)
    {
        _webServerService = webServerService;
        Feature = feature ?? throw new ArgumentNullException(nameof(feature));
        Create = create ?? throw new ArgumentNullException(nameof(create));

        Feature.Disabled += FeatureOnDisabled;
    }

    /// <summary>
    /// The plugin feature that provided the web module.
    /// </summary>
    public PluginFeature Feature { get; }

    /// <summary>
    /// The type of the web module.
    /// </summary>
    public Type? WebModuleType { get; }

    internal Func<IWebModule>? Create { get; }

    internal IWebModule CreateInstance()
    {
        if (Create != null)
            return Create();
        if (WebModuleType != null)
            return (IWebModule) Feature.Plugin.Resolve(WebModuleType);
        throw new ArtemisCoreException("WebModuleRegistration doesn't have a create function nor a web module type :(");
    }

    private void FeatureOnDisabled(object? sender, EventArgs e)
    {
        _webServerService.RemoveModule(this);
        Feature.Disabled -= FeatureOnDisabled;
    }
}
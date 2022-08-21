using System;
using EmbedIO;
using Ninject;

namespace Artemis.Core.Services;

internal class WebModuleRegistration
{
    public WebModuleRegistration(PluginFeature feature, Type webModuleType)
    {
        Feature = feature ?? throw new ArgumentNullException(nameof(feature));
        WebModuleType = webModuleType ?? throw new ArgumentNullException(nameof(webModuleType));
    }

    public WebModuleRegistration(PluginFeature feature, Func<IWebModule> create)
    {
        Feature = feature ?? throw new ArgumentNullException(nameof(feature));
        Create = create ?? throw new ArgumentNullException(nameof(create));
    }

    public PluginFeature Feature { get; }
    public Type? WebModuleType { get; }
    public Func<IWebModule>? Create { get; }

    public IWebModule CreateInstance()
    {
        if (Create != null)
            return Create();
        if (WebModuleType != null)
            return (IWebModule) Feature.Plugin.Kernel!.Get(WebModuleType);
        throw new ArtemisCoreException("WebModuleRegistration doesn't have a create function nor a web module type :(");
    }
}
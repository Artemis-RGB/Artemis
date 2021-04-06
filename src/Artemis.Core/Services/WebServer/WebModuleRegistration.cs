using System;
using EmbedIO;
using Ninject;

namespace Artemis.Core.Services
{
    internal class WebModuleRegistration
    {
        public PluginFeature Feature { get; }
        public Type WebModuleType { get; }

        public WebModuleRegistration(PluginFeature feature, Type webModuleType)
        {
            Feature = feature;
            WebModuleType = webModuleType;
        }

        public IWebModule CreateInstance() => (IWebModule) Feature.Plugin.Kernel!.Get(WebModuleType);
    }
}
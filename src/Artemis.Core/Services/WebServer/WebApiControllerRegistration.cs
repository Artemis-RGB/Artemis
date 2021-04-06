using System;
using EmbedIO.WebApi;
using Ninject;

namespace Artemis.Core.Services
{
    internal class WebApiControllerRegistration<T> : WebApiControllerRegistration where T : WebApiController
    {
        public WebApiControllerRegistration(PluginFeature feature) : base(feature, typeof(T))
        {
            Factory = () => feature.Plugin.Kernel!.Get<T>();
        }

        public Func<T> Factory { get; set; }
        public override object UntypedFactory => Factory;
    }

    internal abstract class WebApiControllerRegistration
    {
        protected WebApiControllerRegistration(PluginFeature feature, Type controllerType)
        {
            Feature = feature;
            ControllerType = controllerType;
        }

        public abstract object UntypedFactory { get; }
        public Type ControllerType { get; set; }
        public PluginFeature Feature { get; }
    }
}
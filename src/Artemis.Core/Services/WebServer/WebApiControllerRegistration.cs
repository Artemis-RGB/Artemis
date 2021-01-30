using System;
using EmbedIO.WebApi;
using Ninject;

namespace Artemis.Core.Services
{
    internal class WebApiControllerRegistration<T> : WebApiControllerRegistration where T : WebApiController
    {
        public WebApiControllerRegistration(IKernel kernel) : base(typeof(T))
        {
            Factory = () => kernel.Get<T>();
        }

        public Func<T> Factory { get; set; }
        public override object UntypedFactory => Factory;
    }

    internal abstract class WebApiControllerRegistration
    {
        protected WebApiControllerRegistration(Type controllerType)
        {
            ControllerType = controllerType;
        }

        public abstract object UntypedFactory { get; }
        public Type ControllerType { get; set; }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Ninject.Activation;
using Ninject.Activation.Providers;
using Ninject.Components;
using Ninject.Infrastructure;
using Ninject.Planning;
using Ninject.Planning.Bindings;
using Ninject.Planning.Bindings.Resolvers;
using Ninject.Selection.Heuristics;

namespace Artemis.UI.Ninject
{
    /// <summary>
    ///     Represents a binding resolver that use the service in question itself as the target to activate but only if the service is a <see cref="UIElement"/>.
    /// </summary>
    public class UIElementSelfBindingResolver : NinjectComponent, IMissingBindingResolver
    {
        private readonly IConstructorScorer constructorScorer;
        private readonly IPlanner planner;

        /// <summary>
        ///     Initializes a new instance of the <see cref="UIElementSelfBindingResolver" /> class.
        /// </summary>
        /// <param name="planner">The <see cref="IPlanner" /> component.</param>
        /// <param name="constructorScorer">The <see cref="IConstructorScorer" /> component.</param>
        public UIElementSelfBindingResolver(IPlanner planner, IConstructorScorer constructorScorer)
        {
            this.planner = planner;
            this.constructorScorer = constructorScorer;
        }

        /// <summary>
        ///     Returns a value indicating whether the specified service is self-bindable.
        /// </summary>
        /// <param name="service">The service.</param>
        /// <returns>
        ///     <see langword="true" /> if the type is self-bindable; otherwise, <see langword="false" />.
        /// </returns>
        protected virtual bool TypeIsSelfBindable(Type service)
        {
            return !service.IsInterface
                   && !service.IsAbstract
                   && !service.IsValueType
                   && service != typeof(string)
                   && !service.ContainsGenericParameters;
        }

        /// <summary>
        ///     Returns any bindings from the specified collection that match the specified service.
        /// </summary>
        /// <param name="bindings">The dictionary of all registered bindings.</param>
        /// <param name="request">The service in question.</param>
        /// <returns>
        ///     The series of matching bindings.
        /// </returns>
        public IEnumerable<IBinding> Resolve(Multimap<Type, IBinding> bindings, IRequest request)
        {
            Type service = request.Service;

            if (!TypeIsSelfBindable(service) || service.IsAssignableFrom(typeof(UIElement))) 
                return Enumerable.Empty<IBinding>();

            return new[]
            {
                new Binding(service)
                {
                    ProviderCallback = ctx => new StandardProvider(service, planner, constructorScorer)
                }
            };
        }
    }
}
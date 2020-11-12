using System;
using Artemis.Core;
using Artemis.UI.Stylet;
using FluentValidation;
using Ninject.Extensions.Conventions;
using Ninject.Modules;
using Ninject.Planning.Bindings.Resolvers;
using Stylet;

namespace Artemis.UI.Ninject
{
    public class PluginUIModule : NinjectModule
    {
        public PluginUIModule(Plugin plugin)
        {
            Plugin = plugin ?? throw new ArgumentNullException(nameof(plugin));
        }

        public Plugin Plugin { get; }

        public override void Load()
        {
            if (Kernel == null)
                throw new ArgumentNullException("Kernel shouldn't be null here.");

            Kernel.Components.Add<IMissingBindingResolver, UIElementSelfBindingResolver>();
            Bind(typeof(IModelValidator<>)).To(typeof(FluentValidationAdapter<>));

            Kernel.Bind(x =>
            {
                x.From(Plugin.Assembly)
                    .SelectAllClasses()
                    .InheritedFrom<IValidator>()
                    .BindAllInterfaces();
            });
        }
    }
}
using System;
using Artemis.Core;
using Artemis.UI.Stylet;
using FluentValidation;
using Ninject.Extensions.Conventions;
using Ninject.Modules;
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
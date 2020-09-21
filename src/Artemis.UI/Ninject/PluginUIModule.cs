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
        public PluginUIModule(PluginInfo pluginInfo)
        {
            PluginInfo = pluginInfo ?? throw new ArgumentNullException(nameof(pluginInfo));
        }

        public PluginInfo PluginInfo { get; }

        public override void Load()
        {
            Bind(typeof(IModelValidator<>)).To(typeof(FluentValidationAdapter<>));
            Kernel.Bind(x =>
            {
                x.From(PluginInfo.Assembly)
                    .SelectAllClasses()
                    .InheritedFrom<IValidator>()
                    .BindAllInterfaces();
            });
        }
    }
}
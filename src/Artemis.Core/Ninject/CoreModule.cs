using System.Linq;
using System.Reflection;
using Artemis.Core.Services.Interfaces;
using Artemis.Plugins.Interfaces;
using Artemis.Plugins.Models;
using Newtonsoft.Json;
using Ninject.Extensions.Conventions;
using Ninject.Modules;

namespace Artemis.Core.Ninject
{
    public class CoreModule : NinjectModule
    {
        public override void Load()
        {
            // Bind all services as singletons
            Kernel.Bind(x =>
            {
                x.FromThisAssembly()
                    .SelectAllClasses()
                    .InheritedFrom<IArtemisService>()
                    .BindAllInterfaces()
                    .Configure(c => c.InSingletonScope());
            });
        }
    }
}
using System.IO;
using Artemis.Core.Exceptions;
using Artemis.Core.Models.Profile.KeyframeEngines;
using Artemis.Core.Plugins.Models;
using Artemis.Core.Services.Interfaces;
using Artemis.Storage.Repositories.Interfaces;
using LiteDB;
using Ninject.Activation;
using Ninject.Extensions.Conventions;
using Ninject.Modules;
using Serilog;

namespace Artemis.Core.Ninject
{
    public class CoreModule : NinjectModule
    {
        public override void Load()
        {
            if (Kernel == null)
                throw new ArtemisCoreException("Failed to bind Ninject Core module, kernel is null.");

            // Bind all services as singletons
            Kernel.Bind(x =>
            {
                x.FromThisAssembly()
                    .SelectAllClasses()
                    .InheritedFrom<IArtemisService>()
                    .BindAllInterfaces()
                    .Configure(c => c.InSingletonScope());
            });

            // Bind all protected services as singletons
            Kernel.Bind(x =>
            {
                x.FromThisAssembly()
                    .SelectAllClasses()
                    .InheritedFrom<IProtectedArtemisService>()
                    .BindAllInterfaces()
                    .Configure(c => c.When(HasAccessToProtectedService).InSingletonScope());
            });

            Kernel.Bind<LiteRepository>().ToMethod(t =>
            {
                // Ensure the data folder exists
                if (!Directory.Exists(Constants.DataFolder))
                    Directory.CreateDirectory(Constants.DataFolder);

                return new LiteRepository(Constants.ConnectionString);
            }).InSingletonScope();

            // Bind all repositories as singletons
            Kernel.Bind(x =>
            {
                x.FromAssemblyContaining<IRepository>()
                    .SelectAllClasses()
                    .InheritedFrom<IRepository>()
                    .BindAllInterfaces()
                    .Configure(c => c.InSingletonScope());
            });

            // Bind all keyframe engines
            Kernel.Bind(x =>
            {
                x.FromAssemblyContaining<KeyframeEngine>()
                    .SelectAllClasses()
                    .InheritedFrom<KeyframeEngine>()
                    .BindAllBaseClasses();
            });

            Kernel.Bind<PluginSettings>().ToProvider<PluginSettingsProvider>();
            Kernel.Bind<ILogger>().ToProvider<LoggerProvider>();
        }

        private bool HasAccessToProtectedService(IRequest r)
        {
            return r.ParentRequest != null && !r.ParentRequest.Service.Assembly.Location.StartsWith(Path.Combine(Constants.DataFolder, "plugins"));
        }
    }
}
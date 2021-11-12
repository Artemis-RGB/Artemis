using Artemis.Core.Services;
using Artemis.Storage;
using Artemis.Storage.Migrations.Interfaces;
using Artemis.Storage.Repositories.Interfaces;
using LiteDB;
using Ninject.Activation;
using Ninject.Extensions.Conventions;
using Ninject.Modules;
using Ninject.Planning.Bindings.Resolvers;
using Serilog;

namespace Artemis.Core.Ninject
{
    /// <summary>
    ///     The main <see cref="NinjectModule" /> of the Artemis Core that binds all services
    /// </summary>
    public class CoreModule : NinjectModule
    {
        /// <inheritdoc />
        public override void Load()
        {
            if (Kernel == null)
                throw new ArtemisCoreException("Failed to bind Ninject Core module, kernel is null.");

            Kernel.Components.Remove<IMissingBindingResolver, SelfBindingResolver>();

            // Bind all services as singletons
            Kernel.Bind(x =>
            {
                x.FromThisAssembly()
                    .IncludingNonPublicTypes()
                    .SelectAllClasses()
                    .InheritedFrom<IArtemisService>()
                    .BindAllInterfaces()
                    .Configure(c => c.InSingletonScope());
            });

            // Bind all protected services as singletons
            Kernel.Bind(x =>
            {
                x.FromThisAssembly()
                    .IncludingNonPublicTypes()
                    .SelectAllClasses()
                    .InheritedFrom<IProtectedArtemisService>()
                    .BindAllInterfaces()
                    .Configure(c => c.When(HasAccessToProtectedService).InSingletonScope());
            });

            Kernel.Bind<LiteRepository>().ToMethod(_ => StorageManager.CreateRepository(Constants.DataFolder)).InSingletonScope();
            Kernel.Bind<StorageMigrationService>().ToSelf().InSingletonScope();

            // Bind all migrations as singletons
            Kernel.Bind(x =>
            {
                x.FromAssemblyContaining<IStorageMigration>()
                    .IncludingNonPublicTypes()
                    .SelectAllClasses()
                    .InheritedFrom<IStorageMigration>()
                    .BindAllInterfaces()
                    .Configure(c => c.InSingletonScope());
            });

            // Bind all repositories as singletons
            Kernel.Bind(x =>
            {
                x.FromAssemblyContaining<IRepository>()
                    .IncludingNonPublicTypes()
                    .SelectAllClasses()
                    .InheritedFrom<IRepository>()
                    .BindAllInterfaces()
                    .Configure(c => c.InSingletonScope());
            });

            Kernel.Bind<PluginSettings>().ToProvider<PluginSettingsProvider>();
            Kernel.Bind<ILogger>().ToProvider<LoggerProvider>();
            Kernel.Bind<LoggerProvider>().ToSelf();
        }

        private bool HasAccessToProtectedService(IRequest r)
        {
            return r.ParentRequest != null && !r.ParentRequest.Service.Assembly.Location.StartsWith(Constants.PluginsFolder);
        }
    }
}
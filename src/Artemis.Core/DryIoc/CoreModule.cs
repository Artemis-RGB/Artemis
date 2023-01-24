using System;
using System.Reflection;
using Artemis.Core.DryIoc.Factories;
using Artemis.Core.Services;
using Artemis.Storage;
using Artemis.Storage.Migrations.Interfaces;
using Artemis.Storage.Repositories.Interfaces;
using DryIoc;

namespace Artemis.Core.DryIoc;

/// <summary>
///     The main <see cref="IModule" /> of the Artemis Core that binds all services
/// </summary>
public class CoreModule : IModule
{
    /// <inheritdoc />
    public void Load(IRegistrator builder)
    {
        Assembly coreAssembly = typeof(IArtemisService).Assembly;
        Assembly storageAssembly = typeof(IRepository).Assembly;

        // Bind all services as singletons
        builder.RegisterMany(new[] {coreAssembly}, type => type.IsAssignableTo<IArtemisService>(), Reuse.Singleton);
        builder.RegisterMany(new[] {coreAssembly}, type => type.IsAssignableTo<IProtectedArtemisService>(), Reuse.Singleton, setup: Setup.With(condition: HasAccessToProtectedService));

        // Bind storage
        builder.RegisterDelegate(() => StorageManager.CreateRepository(Constants.DataFolder), Reuse.Singleton);
        builder.Register<StorageMigrationService>(Reuse.Singleton);
        builder.RegisterMany(new[] {storageAssembly}, type => type.IsAssignableTo<IRepository>(), Reuse.Singleton);

        // Bind migrations
        builder.RegisterMany(new[] {storageAssembly}, type => type.IsAssignableTo<IStorageMigration>());

        builder.Register<IPluginSettingsFactory, PluginSettingsFactory>(Reuse.Singleton);
        builder.Register<IPluginSettingsFactory, PluginSettingsFactory>(Reuse.Singleton);
        builder.Register(made: Made.Of(r => ServiceInfo.Of<IPluginSettingsFactory>(), factory => factory.CreatePluginSettings(Arg.Index<Type>(0))));
        builder.Register(made: Made.Of(r => ServiceInfo.Of<ILoggerFactory>(), factory => factory.CreateLogger(Arg.Index<Type>(0))));
    }

    private bool HasAccessToProtectedService(Request request)
    {
        return request.Parent.ImplementationType != null && !request.Parent.ImplementationType.Assembly.Location.StartsWith(Constants.PluginsFolder);
    }
}
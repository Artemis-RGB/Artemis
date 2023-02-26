using System;
using System.Linq;
using System.Reflection;
using Artemis.Core.DryIoc.Factories;
using Artemis.Core.Services;
using Artemis.Storage;
using Artemis.Storage.Migrations.Interfaces;
using Artemis.Storage.Repositories.Interfaces;
using DryIoc;

namespace Artemis.Core.DryIoc;

/// <summary>
/// Provides an extension method to register services onto a DryIoc <see cref="IContainer"/>.
/// </summary>
public static class ContainerExtensions
{
    /// <summary>
    /// Registers core services into the container.
    /// </summary>
    /// <param name="container">The builder building the current container</param>
    public static void RegisterCore(this IContainer container)
    {
        Assembly[] coreAssembly = {typeof(IArtemisService).Assembly};
        Assembly[] storageAssembly = {typeof(IRepository).Assembly};

        // Bind all services as singletons
        container.RegisterMany(coreAssembly, type => type.IsAssignableTo<IArtemisService>(), Reuse.Singleton);
        container.RegisterMany(coreAssembly, type => type.IsAssignableTo<IProtectedArtemisService>(), Reuse.Singleton, setup: Setup.With(condition: HasAccessToProtectedService));

        // Bind storage
        container.RegisterDelegate(() => StorageManager.CreateRepository(Constants.DataFolder), Reuse.Singleton);
        container.Register<StorageMigrationService>(Reuse.Singleton);
        container.RegisterMany(storageAssembly, type => type.IsAssignableTo<IRepository>(), Reuse.Singleton);

        // Bind migrations
        container.RegisterMany(storageAssembly, type => type.IsAssignableTo<IStorageMigration>(), Reuse.Singleton, nonPublicServiceTypes: true);

        container.Register<IPluginSettingsFactory, PluginSettingsFactory>(Reuse.Singleton);
        container.Register(made: Made.Of(_ => ServiceInfo.Of<IPluginSettingsFactory>(), f => f.CreatePluginSettings(Arg.Index<Type>(0)), r => r.Parent.ImplementationType));
        container.Register<ILoggerFactory, LoggerFactory>(Reuse.Singleton);
        container.Register(made: Made.Of(_ => ServiceInfo.Of<ILoggerFactory>(), f => f.CreateLogger(Arg.Index<Type>(0)), r => r.Parent.ImplementationType));
    }

    /// <summary>
    /// Registers plugin services into the container, this is typically a child container.
    /// </summary>
    /// <param name="container">The builder building the current container</param>
    /// <param name="plugin">The plugin to register</param>
    public static void RegisterPlugin(this IContainer container, Plugin plugin)
    {
        container.RegisterInstance(plugin, setup: Setup.With(preventDisposal: true));

        // Bind plugin service interfaces, DryIoc expects at least one match when calling RegisterMany so ensure there is something to register first
        if (plugin.Assembly != null && plugin.Assembly.GetTypes().Any(t => t.IsAssignableTo<IPluginService>()))
            container.RegisterMany(new[] {plugin.Assembly}, type => type.IsAssignableTo<IPluginService>(), Reuse.Singleton, ifAlreadyRegistered: IfAlreadyRegistered.Keep);
    }

    private static bool HasAccessToProtectedService(Request request)
    {
        // Plugin assembly locations may not be set for some reason, that case it's also not allowed >:(
        return request.Parent.ImplementationType != null &&
               !string.IsNullOrWhiteSpace(request.Parent.ImplementationType.Assembly.Location) &&
               !request.Parent.ImplementationType.Assembly.Location.StartsWith(Constants.PluginsFolder);
    }
}
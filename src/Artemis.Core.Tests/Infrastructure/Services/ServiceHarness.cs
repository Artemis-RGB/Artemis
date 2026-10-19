using Artemis.Core.DryIoc;
using Artemis.Core.DryIoc.Factories;
using Artemis.Storage;
using DryIoc;
using DryIoc.Microsoft.DependencyInjection;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Artemis.Core.Tests.Infrastructure.Services;

/// <summary>
///     A DryIoc container with the Artemis.Core services registered the way the app registers them, but backed by a fresh
///     database in a temporary data folder and with logging going to the test output.
///     <para>
///         Services are created when they are first resolved, nothing is initialized or started. Avoid anything that works
///         on <see cref="Constants.DataFolder" /> or <see cref="Constants.PluginsFolder" /> directly, such as loading plugins
///         or starting the web server, those still point at your real Artemis folders.
///     </para>
/// </summary>
public sealed class ServiceHarness : IDisposable
{
    private bool _disposed;

    /// <param name="configure">Optionally replaces or adds registrations before anything is resolved</param>
    public ServiceHarness(Action<IContainer>? configure = null)
    {
        DataFolder = Path.Combine(Path.GetTempPath(), "Artemis.Core.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(DataFolder);

        // StorageManager only migrates the first database of a process, so migrate this one ourselves
        using (ArtemisDbContext context = CreateDbContext())
            context.Database.Migrate();

        // Same rules as ArtemisBootstrapper
        Container = new Container(rules => rules
            .WithMicrosoftDependencyInjectionRules()
            .WithConcreteTypeDynamicRegistrations()
            .WithoutThrowOnRegisteringDisposableTransient());
        Container.RegisterCore();

        // The real registrations use the database and log folder in your Artemis data folder
        Container.RegisterDelegate(CreateDbContext, Reuse.Transient, ifAlreadyRegistered: IfAlreadyRegistered.Replace);
        Container.Register<ILoggerFactory, TestLoggerFactory>(Reuse.Singleton, ifAlreadyRegistered: IfAlreadyRegistered.Replace);

        configure?.Invoke(Container);
    }

    /// <summary>
    ///     Gets the temporary data folder containing the database, removed when the harness is disposed
    /// </summary>
    public string DataFolder { get; }

    /// <summary>
    ///     Gets the container the services are registered in
    /// </summary>
    public IContainer Container { get; }

    /// <summary>
    ///     Resolves a service, including protected services like the settings service which the container only hands to
    ///     classes outside the plugins folder.
    /// </summary>
    public T Resolve<T>() where T : notnull
    {
        return Container.New<ServiceResolver<T>>().Service;
    }

    /// <summary>
    ///     Creates a database context on the database of this harness, for checking what a service stored
    /// </summary>
    public ArtemisDbContext CreateDbContext()
    {
        return new ArtemisDbContext {DataFolder = DataFolder};
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        Container.Dispose();

        // SQLite keeps the database file open in its connection pool
        SqliteConnection.ClearAllPools();
        try
        {
            Directory.Delete(DataFolder, true);
        }
        catch (IOException)
        {
            // Leave it for the OS to clean up, it is in the temp folder
        }
    }
}

/// <summary>
///     Receives a service through its constructor, the container hands protected services to it since it lives in the test
///     assembly rather than in the plugins folder.
/// </summary>
internal sealed class ServiceResolver<T>(T service)
{
    public T Service { get; } = service;
}

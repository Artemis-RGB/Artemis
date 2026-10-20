using Artemis.Core.DryIoc;
using Artemis.Core.Services;
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
///         The database is a copy of one that is migrated once per test run, and the temporary folders are removed when the
///         test run ends rather than after every test. Both would otherwise make up most of the time service tests take.
///     </para>
///     <para>
///         Services are created when they are first resolved, nothing is initialized or started. Avoid anything that works
///         on <see cref="Constants.DataFolder" /> or <see cref="Constants.PluginsFolder" /> directly, such as loading plugins
///         or starting the web server, those still point at your real Artemis folders.
///     </para>
/// </summary>
public sealed class ServiceHarness : IDisposable
{
    private static readonly string RunFolder = CreateRunFolder();
    private static readonly Lazy<string> TemplateDatabase = new(CreateTemplateDatabase);

    private readonly List<ProfileService> _profileServices = [];
    private bool _disposed;

    /// <param name="configure">Optionally replaces or adds registrations before anything is resolved</param>
    public ServiceHarness(Action<IContainer>? configure = null)
    {
        DataFolder = Path.Combine(RunFolder, Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(DataFolder);
        File.Copy(TemplateDatabase.Value, Path.Combine(DataFolder, "artemis.db"));

        // Same rules as ArtemisBootstrapper
        Container = new Container(rules => rules
            .WithMicrosoftDependencyInjectionRules()
            .WithConcreteTypeDynamicRegistrations()
            .WithoutThrowOnRegisteringDisposableTransient());
        Container.RegisterCore();

        // The real registrations use the database and log folder in your Artemis data folder
        Container.RegisterDelegate(CreateDbContext, Reuse.Transient, ifAlreadyRegistered: IfAlreadyRegistered.Replace);
        Container.Register<ILoggerFactory, TestLoggerFactory>(Reuse.Singleton, ifAlreadyRegistered: IfAlreadyRegistered.Replace);

        // Remember profile services so their profiles can be disposed, see Dispose
        Container.RegisterInitializer<ProfileService>((service, _) => _profileServices.Add(service));

        configure?.Invoke(Container);
    }

    /// <summary>
    ///     Gets the temporary data folder containing the database, removed when the test run ends
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

        // Profiles subscribe to static events of Core's stores, dispose them like the profile service does when it
        // deactivates a profile. Otherwise every profile of every test stays in memory and receives the store events of
        // later tests
        foreach (ProfileConfiguration configuration in _profileServices.SelectMany(s => s.ProfileCategories).SelectMany(c => c.ProfileConfigurations))
        {
            if (configuration.Profile is {Disposed: false} profile)
                profile.Dispose();
            configuration.Dispose();
        }

        Container.Dispose();
    }

    private static string CreateRunFolder()
    {
        string root = Path.Combine(Path.GetTempPath(), "Artemis.Core.Tests");

        // Remove what interrupted test runs left behind, leaving recent folders alone since other test runs may be using them
        if (Directory.Exists(root))
        {
            foreach (DirectoryInfo folder in new DirectoryInfo(root).GetDirectories().Where(d => d.LastWriteTimeUtc < DateTime.UtcNow.AddHours(-1)))
                TryDelete(folder.FullName);
        }

        string runFolder = Path.Combine(root, Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(runFolder);
        AppDomain.CurrentDomain.ProcessExit += (_, _) =>
        {
            // SQLite keeps the database files open in its connection pool
            SqliteConnection.ClearAllPools();
            TryDelete(runFolder);
        };
        return runFolder;
    }

    private static string CreateTemplateDatabase()
    {
        // StorageManager only migrates the first database of a process, so migrate the template ourselves
        string folder = Path.Combine(RunFolder, "template");
        Directory.CreateDirectory(folder);
        using (ArtemisDbContext context = new() {DataFolder = folder})
            context.Database.Migrate();

        // Close the template so it can be copied
        SqliteConnection.ClearAllPools();
        return Path.Combine(folder, "artemis.db");
    }

    private static void TryDelete(string folder)
    {
        try
        {
            Directory.Delete(folder, true);
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            // Left for the next test run, it is in the temp folder
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

using System.Text.Json.Nodes;
using Artemis.Storage;
using Artemis.Storage.Entities;
using Artemis.Storage.Entities.Profile;
using Artemis.Storage.Migrations;
using Artemis.Storage.Migrations.Profile;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Artemis.Core.Tests.Infrastructure.Storage;

/// <summary>
///     Helpers for putting profiles in a test database the way older versions of Artemis left them, and reading back the
///     JSON that is stored
/// </summary>
public static class StoredProfiles
{
    /// <summary>
    ///     Creates the profile migrations Artemis.Core registers
    /// </summary>
    public static List<IProfileMigration> AllMigrations(ILogger logger)
    {
        return [new M0001NodeProviders(), new M0002NodeProvidersProfileConfig(), new M0003SystemTextJson(), new M0004NodeStorage(logger)];
    }

    /// <summary>
    ///     Adds a profile category to the database
    /// </summary>
    public static ProfileCategoryEntity AddCategory(Func<ArtemisDbContext> createContext, string name = "Category")
    {
        using ArtemisDbContext context = createContext();
        ProfileCategoryEntity category = new() {Name = name};
        context.ProfileCategories.Add(category);
        context.SaveChanges();
        return category;
    }

    /// <summary>
    ///     The JSON of <see cref="OldProfileJson.Version0Configuration" /> as the database migration from the old LiteDB
    ///     database left it: still in the old format, but with a version, which that migration always wrote
    /// </summary>
    public static string Version0ConfigurationWithVersion()
    {
        JsonObject configuration = OldProfileJson.Parse(OldProfileJson.Version0Configuration);
        configuration["Version"] = 0;
        return configuration.ToJsonString();
    }

    /// <summary>
    ///     Writes a profile container with the given JSON straight into the database, without going through Entity
    ///     Framework
    /// </summary>
    public static Guid InsertRaw(Func<ArtemisDbContext> createContext, Guid categoryId, string configurationJson, string profileJson)
    {
        Guid id = Guid.NewGuid();
        using ArtemisDbContext context = createContext();
        context.Database.ExecuteSqlRaw(
            "INSERT INTO ProfileContainers (Id, Icon, Profile, ProfileCategoryId, ProfileConfiguration) VALUES ({0}, {1}, {2}, {3}, {4})",
            id, Array.Empty<byte>(), profileJson, categoryId, configurationJson);
        return id;
    }

    /// <summary>
    ///     Reads the JSON of a profile container straight from the database
    /// </summary>
    public static (string Configuration, string Profile) ReadRaw(Func<ArtemisDbContext> createContext, Guid id)
    {
        using ArtemisDbContext context = createContext();
        RawProfileContainer container = context.Database
            .SqlQueryRaw<RawProfileContainer>("SELECT Id, Profile, ProfileConfiguration FROM ProfileContainers WHERE Id = {0}", id)
            .AsEnumerable()
            .Single();
        return (container.ProfileConfiguration, container.Profile);
    }
}

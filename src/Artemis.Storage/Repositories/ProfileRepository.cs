using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Artemis.Storage.Entities;
using Artemis.Storage.Entities.Profile;
using Artemis.Storage.Exceptions;
using Artemis.Storage.Migrations;
using Artemis.Storage.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Artemis.Storage.Repositories;

public class ProfileRepository(ILogger logger, Func<ArtemisDbContext> getContext, List<IProfileMigration> profileMigrators) : IProfileRepository
{
    public void Add(ProfileContainerEntity profileContainerEntity)
    {
        using ArtemisDbContext dbContext = getContext();
        dbContext.ProfileContainers.Add(profileContainerEntity);
        dbContext.SaveChanges();
    }

    public void Remove(ProfileContainerEntity profileContainerEntity)
    {
        using ArtemisDbContext dbContext = getContext();
        dbContext.ProfileContainers.Remove(profileContainerEntity);
        dbContext.SaveChanges();
    }

    public void Save(ProfileContainerEntity profileContainerEntity)
    {
        using ArtemisDbContext dbContext = getContext();
        dbContext.Update(profileContainerEntity);
        dbContext.SaveChanges();
    }

    public void SaveRange(List<ProfileContainerEntity> profileContainerEntities)
    {
        using ArtemisDbContext dbContext = getContext();
        dbContext.UpdateRange(profileContainerEntities);
        dbContext.SaveChanges();
    }

    public void MigrateProfiles()
    {
        using ArtemisDbContext dbContext = getContext();
        int max = profileMigrators.Max(m => m.Version);

        // Query the ProfileContainerEntity table directly, grabbing the ID, profile, and configuration
        List<RawProfileContainer> containers = dbContext.Database
            .SqlQueryRaw<RawProfileContainer>("SELECT Id, Profile, ProfileConfiguration FROM ProfileContainers WHERE json_extract(ProfileConfiguration, '$.Version') < {0}", max)
            .ToList();

        foreach (RawProfileContainer rawProfileContainer in containers)
        {
            JsonObject? profileConfiguration = JsonNode.Parse(rawProfileContainer.ProfileConfiguration)?.AsObject();
            JsonObject? profile = JsonNode.Parse(rawProfileContainer.Profile)?.AsObject();
            
            if (profileConfiguration == null || profile == null)
                throw new ArtemisStorageException("Failed to parse profile or profile configuration");

            MigrateProfile(profileConfiguration, profile);
            rawProfileContainer.Profile = profile.ToString();
            rawProfileContainer.ProfileConfiguration = profileConfiguration.ToString();

            // Write the updated containers back to the database
            dbContext.Database.ExecuteSqlRaw(
                "UPDATE ProfileContainers SET Profile = {0}, ProfileConfiguration = {1} WHERE Id = {2}",
                rawProfileContainer.Profile,
                rawProfileContainer.ProfileConfiguration,
                rawProfileContainer.Id);
        }
    }

    public void MigrateProfile(JsonObject? configurationJson, JsonObject? profileJson)
    {
        if (configurationJson == null || profileJson == null)
            return;

        configurationJson["Version"] ??= 0;

        foreach (IProfileMigration profileMigrator in profileMigrators.OrderBy(m => m.Version))
        {
            if (profileMigrator.Version <= configurationJson["Version"]!.GetValue<int>())
                continue;
            
            logger.Information("Migrating profile from version {OldVersion} to {NewVersion}", configurationJson["Version"], profileMigrator.Version);

            profileMigrator.Migrate(configurationJson, profileJson);
            configurationJson["Version"] = profileMigrator.Version;
        }
    }
}
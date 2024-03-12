using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Storage.Entities.Profile;
using Artemis.Storage.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Artemis.Storage.Repositories;

internal class ProfileCategoryRepository(Func<ArtemisDbContext> getContext, IProfileRepository profileRepository) : IProfileCategoryRepository
{
    private bool _migratedProfiles;

    public void Add(ProfileCategoryEntity profileCategoryEntity)
    {
        using ArtemisDbContext dbContext = getContext();
        dbContext.ProfileCategories.Add(profileCategoryEntity);
        dbContext.SaveChanges();
    }

    public void Remove(ProfileCategoryEntity profileCategoryEntity)
    {
        using ArtemisDbContext dbContext = getContext();
        dbContext.ProfileCategories.Remove(profileCategoryEntity);
        dbContext.SaveChanges();
    }

    public List<ProfileCategoryEntity> GetAll()
    {
        if (!_migratedProfiles)
        {
            profileRepository.MigrateProfiles();
            _migratedProfiles = true;
        }

        using ArtemisDbContext dbContext = getContext();
        return dbContext.ProfileCategories.Include(c => c.ProfileConfigurations).ToList();
    }

    public ProfileCategoryEntity? Get(Guid id)
    {
        using ArtemisDbContext dbContext = getContext();
        return dbContext.ProfileCategories.Include(c => c.ProfileConfigurations).FirstOrDefault(c => c.Id == id);
    }

    public void Save(ProfileCategoryEntity profileCategoryEntity)
    {
        using ArtemisDbContext dbContext = getContext();
        dbContext.Update(profileCategoryEntity);
        dbContext.SaveChanges();
    }

    public void SaveRange(List<ProfileCategoryEntity> profileCategoryEntities)
    {
        using ArtemisDbContext dbContext = getContext();
        dbContext.UpdateRange(profileCategoryEntities);
        dbContext.SaveChanges();
    }

    public bool IsUnique(string name, Guid? id)
    {
        using ArtemisDbContext dbContext = getContext();

        name = name.Trim();
        return id == null
            ? dbContext.ProfileCategories.Any(p => p.Name == name)
            : dbContext.ProfileCategories.Any(p => p.Name == name && p.Id != id.Value);
    }
}
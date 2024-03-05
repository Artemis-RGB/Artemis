using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Storage.Entities.Profile;
using Artemis.Storage.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Artemis.Storage.Repositories;

internal class ProfileCategoryRepository : IProfileCategoryRepository
{
    private readonly ArtemisDbContext _dbContext;

    public ProfileCategoryRepository(ArtemisDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Add(ProfileCategoryEntity profileCategoryEntity)
    {
        _dbContext.ProfileCategories.Add(profileCategoryEntity);
        SaveChanges();
    }

    public void Remove(ProfileCategoryEntity profileCategoryEntity)
    {
        _dbContext.ProfileCategories.Remove(profileCategoryEntity);
        SaveChanges();
    }

    public List<ProfileCategoryEntity> GetAll()
    {
        return _dbContext.ProfileCategories.Include(c => c.ProfileConfigurations).ToList();
    }

    public ProfileCategoryEntity? Get(Guid id)
    {
        return _dbContext.ProfileCategories.Include(c => c.ProfileConfigurations).FirstOrDefault(c => c.Id == id);
    }

    public bool IsUnique(string name, Guid? id)
    {
        name = name.Trim();
        if (id == null)
            return _dbContext.ProfileCategories.Any(p => p.Name == name);
        return _dbContext.ProfileCategories.Any(p => p.Name == name && p.Id != id.Value);
    }
    
    public void SaveChanges()
    {
        _dbContext.SaveChanges();
    }
}
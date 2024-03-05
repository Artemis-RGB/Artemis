using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Storage.Entities.Profile;
using Artemis.Storage.Repositories.Interfaces;
using LiteDB;

namespace Artemis.Storage.Repositories;

internal class ProfileRepository : IProfileRepository
{
    private readonly ArtemisDbContext _dbContext;

    public ProfileRepository(ArtemisDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Add(ProfileContainerEntity profileContainerEntity)
    {
        _dbContext.Profiles.Add(profileContainerEntity);
        SaveChanges();
    }

    public void Remove(ProfileContainerEntity profileContainerEntity)
    {
        _dbContext.Profiles.Remove(profileContainerEntity);
        SaveChanges();
    }

    public List<ProfileContainerEntity> GetAll()
    {
        return _dbContext.Profiles.ToList();
    }

    public ProfileContainerEntity? Get(Guid id)
    {
        return _dbContext.Profiles.FirstOrDefault(c => c.Profile.Id == id);
    }

    public void SaveChanges()
    {
        _dbContext.SaveChanges();
    }
}
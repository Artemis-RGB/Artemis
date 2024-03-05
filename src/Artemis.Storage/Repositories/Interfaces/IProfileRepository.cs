using System;
using System.Collections.Generic;
using Artemis.Storage.Entities.Profile;

namespace Artemis.Storage.Repositories.Interfaces;

public interface IProfileRepository : IRepository
{
    void Add(ProfileContainerEntity profileContainerEntity);
    void Remove(ProfileContainerEntity profileContainerEntity);
    List<ProfileContainerEntity> GetAll();
    ProfileContainerEntity? Get(Guid id);
    void SaveChanges();
}
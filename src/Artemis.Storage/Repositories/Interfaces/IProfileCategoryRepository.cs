using System;
using System.Collections.Generic;
using System.IO;
using Artemis.Storage.Entities.Profile;

namespace Artemis.Storage.Repositories.Interfaces;

public interface IProfileCategoryRepository : IRepository
{
    void Add(ProfileCategoryEntity profileCategoryEntity);
    void Remove(ProfileCategoryEntity profileCategoryEntity);
    List<ProfileCategoryEntity> GetAll();
    ProfileCategoryEntity? Get(Guid id);
    bool IsUnique(string name, Guid? id);
    void Save(ProfileCategoryEntity profileCategoryEntity);
    void SaveRange(List<ProfileCategoryEntity> profileCategoryEntities);
}
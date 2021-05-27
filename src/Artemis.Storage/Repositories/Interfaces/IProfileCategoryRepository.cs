using System;
using System.Collections.Generic;
using Artemis.Storage.Entities.Profile;

namespace Artemis.Storage.Repositories.Interfaces
{
    public interface IProfileCategoryRepository : IRepository
    {
        void Add(ProfileCategoryEntity profileCategoryEntity);
        void Remove(ProfileCategoryEntity profileCategoryEntity);
        List<ProfileCategoryEntity> GetAll();
        ProfileCategoryEntity Get(Guid id);
        void Save(ProfileCategoryEntity profileCategoryEntity);
    }
}
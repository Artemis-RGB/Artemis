using System;
using System.Collections.Generic;
using Artemis.Storage.Entities;
using Artemis.Storage.Entities.Profile;

namespace Artemis.Storage.Repositories.Interfaces
{
    public interface IProfileRepository : IRepository
    {
        void Add(ProfileEntity profileEntity);
        void Remove(ProfileEntity profileEntity);
        List<ProfileEntity> GetAll();
        ProfileEntity Get(Guid id);
        List<ProfileEntity> GetByPluginGuid(Guid pluginGuid);
        void Save(ProfileEntity profileEntity);
    }
}
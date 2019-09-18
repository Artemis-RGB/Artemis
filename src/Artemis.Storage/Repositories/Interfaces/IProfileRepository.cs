using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Artemis.Storage.Entities;

namespace Artemis.Storage.Repositories.Interfaces
{
    public interface IProfileRepository : IRepository
    {
        IQueryable<ProfileEntity> GetAll();
        Task<IList<ProfileEntity>> GetByPluginGuidAsync(Guid pluginGuid);
        Task<ProfileEntity> GetByGuidAsync(string guid);
        void Save();
        Task SaveAsync();
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Artemis.Storage.Entities;

namespace Artemis.Storage.Repositories
{
    public interface ISettingRepository : IRepository
    {
        IQueryable<SettingEntity> GetAll();
        List<SettingEntity> GetByPluginGuid(Guid pluginGuid);
        void Add(SettingEntity settingEntity);
        Task<List<SettingEntity>> GetByPluginGuidAsync(Guid pluginGuid);
        Task<SettingEntity> GetByNameAndPluginGuid(string name, Guid pluginGuid);
        Task<SettingEntity> GetByName(string name);
        void Save();
        Task SaveAsync();
    }
}
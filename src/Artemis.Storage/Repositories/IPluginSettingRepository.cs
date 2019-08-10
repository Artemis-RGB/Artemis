using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Artemis.Storage.Entities;

namespace Artemis.Storage.Repositories
{
    public interface IPluginSettingRepository : IRepository
    {
        void Add(PluginSettingEntity pluginSettingEntity);
        List<PluginSettingEntity> GetByPluginGuid(Guid pluginGuid);
        Task<List<PluginSettingEntity>> GetByPluginGuidAsync(Guid pluginGuid);
        PluginSettingEntity GetByNameAndPluginGuid(string name, Guid pluginGuid);
        Task<PluginSettingEntity> GetByNameAndPluginGuidAsync(string name, Guid pluginGuid);
        void Save();
        Task SaveAsync();
    }
}
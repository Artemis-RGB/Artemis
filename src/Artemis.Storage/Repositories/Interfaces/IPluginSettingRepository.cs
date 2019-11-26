using System;
using System.Collections.Generic;
using Artemis.Storage.Entities;

namespace Artemis.Storage.Repositories.Interfaces
{
    public interface IPluginSettingRepository : IRepository
    {
        void Add(PluginSettingEntity pluginSettingEntity);
        List<PluginSettingEntity> GetByPluginGuid(Guid pluginGuid);
        PluginSettingEntity GetByNameAndPluginGuid(string name, Guid pluginGuid);
        void Save(PluginSettingEntity pluginSettingEntity);
    }
}
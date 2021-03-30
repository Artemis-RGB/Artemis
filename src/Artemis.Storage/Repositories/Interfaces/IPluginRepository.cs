using System;
using System.Collections.Generic;
using Artemis.Storage.Entities.Plugins;

namespace Artemis.Storage.Repositories.Interfaces
{
    public interface IPluginRepository : IRepository
    {
        void AddPlugin(PluginEntity pluginEntity);
        PluginEntity GetPluginByGuid(Guid pluginGuid);
        void SavePlugin(PluginEntity pluginEntity);
        
        void AddSetting(PluginSettingEntity pluginSettingEntity);
        PluginSettingEntity GetSettingByGuid(Guid pluginGuid);
        PluginSettingEntity GetSettingByNameAndGuid(string name, Guid pluginGuid);
        void SaveSetting(PluginSettingEntity pluginSettingEntity);
        void RemoveSettings(Guid pluginGuid);
        
        void AddQueuedAction(PluginQueuedActionEntity pluginQueuedActionEntity);
        List<PluginQueuedActionEntity> GetQueuedActions();
        List<PluginQueuedActionEntity> GetQueuedActions(Guid pluginGuid);
        void RemoveQueuedAction(PluginQueuedActionEntity pluginQueuedActionEntity);
    }
}
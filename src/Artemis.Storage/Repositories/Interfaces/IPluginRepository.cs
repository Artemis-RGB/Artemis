using System;
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
    }
}
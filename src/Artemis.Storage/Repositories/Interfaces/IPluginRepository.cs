using System;
using Artemis.Storage.Entities.Plugins;

namespace Artemis.Storage.Repositories.Interfaces;

public interface IPluginRepository : IRepository
{
    PluginEntity? GetPluginByPluginGuid(Guid pluginGuid);
    void SaveSetting(PluginSettingEntity pluginSettingEntity);
    void SavePlugin(PluginEntity pluginEntity);
    PluginSettingEntity? GetSettingByNameAndGuid(string name, Guid pluginGuid);
    void RemoveSettings(Guid pluginGuid);
}
using System;
using Artemis.Storage.Entities.Plugins;

namespace Artemis.Storage.Repositories.Interfaces;

public interface IPluginRepository : IRepository
{
    void AddPlugin(PluginEntity pluginEntity);
    PluginEntity? GetPluginByGuid(Guid pluginGuid);
    void AddSetting(PluginSettingEntity pluginSettingEntity);
    PluginSettingEntity? GetSettingByNameAndGuid(string name, Guid pluginGuid);
    void RemoveSettings(Guid pluginGuid);
    void SaveChanges();
}
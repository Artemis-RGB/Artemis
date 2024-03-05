using System;
using System.Linq;
using Artemis.Storage.Entities.Plugins;
using Artemis.Storage.Repositories.Interfaces;

namespace Artemis.Storage.Repositories;

internal class PluginRepository : IPluginRepository
{
    private readonly ArtemisDbContext _dbContext;

    public PluginRepository(ArtemisDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void AddPlugin(PluginEntity pluginEntity)
    {
        _dbContext.Plugins.Add(pluginEntity);
        SaveChanges();
    }

    public PluginEntity? GetPluginByGuid(Guid pluginGuid)
    {
        return _dbContext.Plugins.FirstOrDefault(p => p.Id == pluginGuid);
    }
    
    public void AddSetting(PluginSettingEntity pluginSettingEntity)
    {
        _dbContext.PluginSettings.Add(pluginSettingEntity);
        SaveChanges();
    }

    public PluginSettingEntity? GetSettingByGuid(Guid pluginGuid)
    {
        return _dbContext.PluginSettings.FirstOrDefault(p => p.PluginGuid == pluginGuid);
    }

    public PluginSettingEntity? GetSettingByNameAndGuid(string name, Guid pluginGuid)
    {
        return _dbContext.PluginSettings.FirstOrDefault(p => p.Name == name && p.PluginGuid == pluginGuid);
    }
    
    public void RemoveSettings(Guid pluginGuid)
    {
        _dbContext.PluginSettings.RemoveRange(_dbContext.PluginSettings.Where(s => s.PluginGuid == pluginGuid));
    }
    
    public void SaveChanges()
    {
        _dbContext.SaveChanges();
    }
}
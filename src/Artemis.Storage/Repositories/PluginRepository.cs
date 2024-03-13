using System;
using System.Linq;
using Artemis.Storage.Entities.Plugins;
using Artemis.Storage.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Artemis.Storage.Repositories;

internal class PluginRepository(Func<ArtemisDbContext> getContext) : IPluginRepository
{
    public PluginEntity? GetPluginByPluginGuid(Guid pluginGuid)
    {
        using ArtemisDbContext dbContext = getContext();
        return dbContext.Plugins.Include(p => p.Features).FirstOrDefault(p => p.PluginGuid == pluginGuid);
    }

    public PluginSettingEntity? GetSettingByNameAndGuid(string name, Guid pluginGuid)
    {
        using ArtemisDbContext dbContext = getContext();
        return dbContext.PluginSettings.FirstOrDefault(p => p.Name == name && p.PluginGuid == pluginGuid);
    }

    public void RemoveSettings(Guid pluginGuid)
    {
        using ArtemisDbContext dbContext = getContext();
        dbContext.PluginSettings.RemoveRange(dbContext.PluginSettings.Where(s => s.PluginGuid == pluginGuid));
        dbContext.SaveChanges();
    }
    
    public void SaveSetting(PluginSettingEntity pluginSettingEntity)
    {
        using ArtemisDbContext dbContext = getContext();
        dbContext.PluginSettings.Update(pluginSettingEntity);
        dbContext.SaveChanges();
    }

    public void SavePlugin(PluginEntity pluginEntity)
    {
        using ArtemisDbContext dbContext = getContext();
        dbContext.Update(pluginEntity);
        dbContext.SaveChanges();
    }

}
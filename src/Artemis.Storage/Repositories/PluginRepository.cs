using System;
using Artemis.Storage.Entities.Plugins;
using Artemis.Storage.Repositories.Interfaces;
using LiteDB;

namespace Artemis.Storage.Repositories;

internal class PluginRepository : IPluginRepository
{
    private readonly LiteRepository _repository;

    public PluginRepository(LiteRepository repository)
    {
        _repository = repository;

        _repository.Database.GetCollection<PluginSettingEntity>().EnsureIndex(s => new {s.Name, s.PluginGuid}, true);
    }

    public void AddPlugin(PluginEntity pluginEntity)
    {
        _repository.Insert(pluginEntity);
    }

    public PluginEntity? GetPluginByGuid(Guid pluginGuid)
    {
        return _repository.FirstOrDefault<PluginEntity>(p => p.Id == pluginGuid);
    }

    public void SavePlugin(PluginEntity pluginEntity)
    {
        _repository.Upsert(pluginEntity);
    }

    public void AddSetting(PluginSettingEntity pluginSettingEntity)
    {
        _repository.Insert(pluginSettingEntity);
    }

    public PluginSettingEntity? GetSettingByGuid(Guid pluginGuid)
    {
        return _repository.FirstOrDefault<PluginSettingEntity>(p => p.PluginGuid == pluginGuid);
    }

    public PluginSettingEntity? GetSettingByNameAndGuid(string name, Guid pluginGuid)
    {
        return _repository.FirstOrDefault<PluginSettingEntity>(p => p.Name == name && p.PluginGuid == pluginGuid);
    }

    public void SaveSetting(PluginSettingEntity pluginSettingEntity)
    {
        _repository.Upsert(pluginSettingEntity);
    }

    /// <inheritdoc />
    public void RemoveSettings(Guid pluginGuid)
    {
        _repository.DeleteMany<PluginSettingEntity>(s => s.PluginGuid == pluginGuid);
    }
}
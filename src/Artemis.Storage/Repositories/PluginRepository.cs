using System;
using System.Collections.Generic;
using Artemis.Storage.Entities.Plugins;
using Artemis.Storage.Repositories.Interfaces;
using LiteDB;

namespace Artemis.Storage.Repositories
{
    internal class PluginRepository : IPluginRepository
    {
        private readonly LiteRepository _repository;

        public PluginRepository(LiteRepository repository)
        {
            _repository = repository;

            _repository.Database.GetCollection<PluginSettingEntity>().EnsureIndex(s => new {s.Name, s.PluginGuid}, true);
            _repository.Database.GetCollection<PluginQueuedActionEntity>().EnsureIndex(s => s.PluginGuid);
        }

        public void AddPlugin(PluginEntity pluginEntity)
        {
            _repository.Insert(pluginEntity);
        }

        public PluginEntity GetPluginByGuid(Guid pluginGuid)
        {
            return _repository.FirstOrDefault<PluginEntity>(p => p.Id == pluginGuid);
        }

        public void SavePlugin(PluginEntity pluginEntity)
        {
            _repository.Upsert(pluginEntity);
            _repository.Database.Checkpoint();
        }

        public void AddSetting(PluginSettingEntity pluginSettingEntity)
        {
            _repository.Insert(pluginSettingEntity);
        }

        public PluginSettingEntity GetSettingByGuid(Guid pluginGuid)
        {
            return _repository.FirstOrDefault<PluginSettingEntity>(p => p.PluginGuid == pluginGuid);
        }

        public PluginSettingEntity GetSettingByNameAndGuid(string name, Guid pluginGuid)
        {
            return _repository.FirstOrDefault<PluginSettingEntity>(p => p.Name == name && p.PluginGuid == pluginGuid);
        }

        public void SaveSetting(PluginSettingEntity pluginSettingEntity)
        {
            _repository.Upsert(pluginSettingEntity);
        }

        public List<PluginQueuedActionEntity> GetQueuedActions()
        {
            return _repository.Query<PluginQueuedActionEntity>().ToList();
        }

        public List<PluginQueuedActionEntity> GetQueuedActions(Guid pluginGuid)
        {
            return _repository.Query<PluginQueuedActionEntity>().Where(q => q.PluginGuid == pluginGuid).ToList();
        }

        public void AddQueuedAction(PluginQueuedActionEntity pluginQueuedActionEntity)
        {
            _repository.Upsert(pluginQueuedActionEntity);
        }
        public void RemoveQueuedAction(PluginQueuedActionEntity pluginQueuedActionEntity)
        {
            _repository.Delete<PluginQueuedActionEntity>(pluginQueuedActionEntity.Id);
        }
    }
}
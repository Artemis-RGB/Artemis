using System;
using System.Collections.Generic;
using Artemis.Storage.Entities;
using Artemis.Storage.Repositories.Interfaces;
using LiteDB;

namespace Artemis.Storage.Repositories
{
    public class PluginSettingRepository : IPluginSettingRepository
    {
        private readonly LiteRepository _repository;

        internal PluginSettingRepository(LiteRepository repository)
        {
            _repository = repository;
            _repository.Database.GetCollection<PluginSettingEntity>().EnsureIndex(s => s.Name);
            _repository.Database.GetCollection<PluginSettingEntity>().EnsureIndex(s => s.PluginGuid);
        }

        public void Add(PluginSettingEntity pluginSettingEntity)
        {
            _repository.Insert(pluginSettingEntity);
        }

        public List<PluginSettingEntity> GetByPluginGuid(Guid pluginGuid)
        {
            return _repository.Query<PluginSettingEntity>().Where(p => p.PluginGuid == pluginGuid).ToList();
        }

        public PluginSettingEntity GetByNameAndPluginGuid(string name, Guid pluginGuid)
        {
            return _repository.FirstOrDefault<PluginSettingEntity>(p => p.Name == name && p.PluginGuid == pluginGuid);
        }

        public void Save(PluginSettingEntity pluginSettingEntity)
        {
            _repository.Upsert(pluginSettingEntity);
        }
    }
}
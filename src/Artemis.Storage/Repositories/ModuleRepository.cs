using System;
using System.Collections.Generic;
using Artemis.Storage.Entities.Module;
using Artemis.Storage.Repositories.Interfaces;
using LiteDB;

namespace Artemis.Storage.Repositories
{
    public class ModuleRepository : IModuleRepository
    {
        private readonly LiteRepository _repository;

        internal ModuleRepository(LiteRepository repository)
        {
            _repository = repository;
            _repository.Database.GetCollection<ModuleSettingsEntity>().EnsureIndex(s => s.PluginGuid);
        }

        public void Add(ModuleSettingsEntity moduleSettingsEntity)
        {
            _repository.Insert(moduleSettingsEntity);
        }

        public ModuleSettingsEntity GetByPluginGuid(Guid guid)
        {
            return _repository.FirstOrDefault<ModuleSettingsEntity>(s => s.PluginGuid == guid);
        }

        public List<ModuleSettingsEntity> GetAll()
        {
            return _repository.Query<ModuleSettingsEntity>().ToList();
        }

        public List<ModuleSettingsEntity> GetByCategory(int category)
        {
            return _repository.Query<ModuleSettingsEntity>().Where(s => s.PriorityCategory == category).ToList();
        }

        public void Save(ModuleSettingsEntity moduleSettingsEntity)
        {
            _repository.Upsert(moduleSettingsEntity);
        }
    }
}
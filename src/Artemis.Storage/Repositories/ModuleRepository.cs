using System;
using System.Collections.Generic;
using Artemis.Storage.Entities.Module;
using Artemis.Storage.Repositories.Interfaces;
using LiteDB;

namespace Artemis.Storage.Repositories
{
    internal class ModuleRepository : IModuleRepository
    {
        private readonly LiteRepository _repository;

        public ModuleRepository(LiteRepository repository)
        {
            _repository = repository;
            _repository.Database.GetCollection<ModuleSettingsEntity>().EnsureIndex(s => s.ModuleId, true);
        }

        public void Add(ModuleSettingsEntity moduleSettingsEntity)
        {
            _repository.Insert(moduleSettingsEntity);
        }

        public ModuleSettingsEntity GetByModuleId(string moduleId)
        {
            return _repository.FirstOrDefault<ModuleSettingsEntity>(s => s.ModuleId == moduleId);
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
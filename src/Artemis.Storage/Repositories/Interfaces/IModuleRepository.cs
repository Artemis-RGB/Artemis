using System;
using System.Collections.Generic;
using Artemis.Storage.Entities.Module;

namespace Artemis.Storage.Repositories.Interfaces
{
    public interface IModuleRepository : IRepository
    {
        void Add(ModuleSettingsEntity moduleSettingsEntity);
        ModuleSettingsEntity GetByPluginGuid(Guid guid);
        List<ModuleSettingsEntity> GetAll();
        List<ModuleSettingsEntity> GetByCategory(int category);
        void Save(ModuleSettingsEntity moduleSettingsEntity);
    }
}
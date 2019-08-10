using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Artemis.Storage.Entities;
using Microsoft.EntityFrameworkCore;

namespace Artemis.Storage.Repositories
{
    public class PluginSettingRepository : IPluginSettingRepository
    {
        private readonly StorageContext _dbContext;

        internal PluginSettingRepository()
        {
            _dbContext = new StorageContext();
            _dbContext.Database.EnsureCreated();
        }

        public void Add(PluginSettingEntity pluginSettingEntity)
        {
            _dbContext.PluginSettings.Add(pluginSettingEntity);
        }

        public List<PluginSettingEntity> GetByPluginGuid(Guid pluginGuid)
        {
            return _dbContext.PluginSettings.Where(p => p.PluginGuid == pluginGuid).ToList();
        }

        public async Task<List<PluginSettingEntity>> GetByPluginGuidAsync(Guid pluginGuid)
        {
            return await _dbContext.PluginSettings.Where(p => p.PluginGuid == pluginGuid).ToListAsync();
        }

        public PluginSettingEntity GetByNameAndPluginGuid(string name, Guid pluginGuid)
        {
            return _dbContext.PluginSettings.FirstOrDefault(p => p.Name == name && p.PluginGuid == pluginGuid);
        }

        public async Task<PluginSettingEntity> GetByNameAndPluginGuidAsync(string name, Guid pluginGuid)
        {
            return await _dbContext.PluginSettings.FirstOrDefaultAsync(p => p.Name == name && p.PluginGuid == pluginGuid);
        }

        public void Save()
        {
            _dbContext.SaveChanges();
        }

        public async Task SaveAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}
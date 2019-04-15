using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Artemis.Storage.Entities;
using Microsoft.EntityFrameworkCore;

namespace Artemis.Storage.Repositories
{
    public class SettingRepository
    {
        private readonly StorageContext _dbContext;

        public SettingRepository()
        {
            _dbContext = new StorageContext();
        }

        public IQueryable<SettingEntity> GetAll()
        {
            return _dbContext.Settings;
        }

        public async Task<List<SettingEntity>> GetByPluginGuid(Guid pluginGuid)
        {
            return await _dbContext.Settings.Where(p => p.PluginGuid == pluginGuid).ToListAsync();
        }

        public async Task<SettingEntity> GetByNameAndPluginGuid(string name, Guid pluginGuid)
        {
            return await _dbContext.Settings.FirstOrDefaultAsync(p => p.Name == name && p.PluginGuid == pluginGuid);
        }

        public async Task<SettingEntity> GetByName(string name)
        {
            return await _dbContext.Settings.FirstOrDefaultAsync(p => p.Name == name);
        }

        public async Task SaveAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}
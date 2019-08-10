using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Artemis.Storage.Entities;
using Microsoft.EntityFrameworkCore;

namespace Artemis.Storage.Repositories
{
    public class SettingRepository : ISettingRepository
    {
        private readonly StorageContext _dbContext;

        internal SettingRepository()
        {
            _dbContext = new StorageContext();
            _dbContext.Database.EnsureCreated();
        }

        public void Add(SettingEntity settingEntity)
        {
            _dbContext.Settings.Add(settingEntity);
        }

        public SettingEntity Get(string name)
        {
            return _dbContext.Settings.FirstOrDefault(p => p.Name == name);
        }

        public async Task<SettingEntity> GetAsync(string name)
        {
            return await _dbContext.Settings.FirstOrDefaultAsync(p => p.Name == name);
        }

        public List<SettingEntity> GetAll()
        {
            return _dbContext.Settings.ToList();
        }

        public async Task<List<SettingEntity>> GetAllAsync()
        {
            return await _dbContext.Settings.ToListAsync();
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